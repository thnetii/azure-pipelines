#!/usr/bin/env node
const path = require('path');
const {
  command,
  debug,
  getVariable,
  setResult,
  TaskResult,
} = require('azure-pipelines-task-lib');
const { ToolRunner } = require('azure-pipelines-task-lib/toolrunner');

const argv = process.argv.slice(2);

const sourcesRootDirectory =
  getVariable('Build.SourcesDirectory') ||
  process.env['Build.SourcesDirectory'] ||
  process.cwd();

const runTracker = {
  warningCount: 0,
  errorCount: 0,
};
const tscRunner = new ToolRunner('npm');
tscRunner.arg(['exec', '--', 'tsc']);
tscRunner.arg(argv);

/** @see https://github.com/microsoft/vscode/blob/7db1a2b88f7557e0a43fec75b6ba7e50b3e9f77e/extensions/typescript-language-features/package.json#L1296 */
const tscRegexPattern =
  '^([^\\s].*)[\\(:](\\d+)[,:](\\d+)(?:\\):\\s+|\\s+-\\s+)(error|warning|info)\\s+TS(\\d+)\\s*:\\s*(.*)$';
const tscRegexMatcher = new RegExp(tscRegexPattern, 'u');
const tscShortRegexMatcher = /^(error|warning|info)\s+TS(\d+)\s*:\s*(.*)$/u;

/**
 * @param {string} line
 */
function getTscFullRegexInfo(line) {
  const tscOutputMatch = tscRegexMatcher.exec(line);
  if (!tscOutputMatch) return false;
  const [, file, lineno, column, severity = '', code, message = ''] =
    tscOutputMatch;
  const tscCmdProps = {
    sourcepath: file ? path.relative(sourcesRootDirectory, file) : file,
    type: severity,
    linenumber: lineno ? parseInt(lineno, 10) : undefined,
    columnnumber: column ? parseInt(column, 10) : undefined,
    code: `TS${code}`,
  };
  return {
    message,
    props: tscCmdProps,
  };
}

/**
 * @param {string} line
 */
function getTscShortRegexInfo(line) {
  const tscOutputMatch = tscShortRegexMatcher.exec(line);
  if (!tscOutputMatch) return false;
  const [, severity = '', code, message = ''] = tscOutputMatch;
  const tscCmdProps = {
    type: severity,
    code: `TS${code}`,
  };
  return {
    message,
    props: tscCmdProps,
  };
}

tscRunner.on(
  'stdline',
  /** @param {string} line */ (line) => {
    const info = getTscFullRegexInfo(line) || getTscShortRegexInfo(line);
    if (!info) return;
    if (/^warning$/iu.test(info.props.type)) {
      runTracker.warningCount += 1;
    } else if (/^error$/iu.test(info.props.type)) {
      runTracker.errorCount += 1;
    }
    command('task.logissue', info.props, info.message);
  }
);

tscRunner.exec({ ignoreReturnCode: true }).then((exitCode) => {
  let result = TaskResult.Succeeded;
  if (runTracker.warningCount) {
    result = TaskResult.SucceededWithIssues;
  }
  if (runTracker.errorCount) {
    result = TaskResult.Failed;
  }
  setResult(result, '');
  debug(`TSC exited with code '${exitCode}'.`);
});
