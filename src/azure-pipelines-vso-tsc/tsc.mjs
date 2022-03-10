#!/usr/bin/env node
import path from 'path';
import {
  command, debug, getVariable, setResult, TaskResult
} from 'azure-pipelines-task-lib';
import { _writeLine } from 'azure-pipelines-task-lib/internal.js';
import { ToolRunner } from 'azure-pipelines-task-lib/toolrunner.js';
const argv = process.argv.slice(2);

const sourcesRootDirectory = getVariable('Build.SourcesDirectory')
  || process.env['Build.SourcesDirectory'] || path.resolve();

const runTracker = {
  warningCount: 0,
  errorCount: 0
};
const tscRunner = new ToolRunner('npx');
tscRunner.arg('tsc');
tscRunner.arg(argv);

/** @see https://github.com/microsoft/vscode/blob/7db1a2b88f7557e0a43fec75b6ba7e50b3e9f77e/extensions/typescript-language-features/package.json#L1296 */
const tscRegexPattern = '^([^\\s].*)[\\(:](\\d+)[,:](\\d+)(?:\\):\\s+|\\s+-\\s+)(error|warning|info)\\s+TS(\\d+)\\s*:\\s*(.*)$';
const tscRegexMatcher = new RegExp(tscRegexPattern, 'u');

tscRunner.on('stdline', /** @param {string} line */ line => {
  const tscOutputMatch = tscRegexMatcher.exec(line);
  if (!tscOutputMatch) {
    return;
  }
  const [, file, lineno, column, severity, code, message] = tscOutputMatch;
  const tscCmdProps = {
    sourcepath: path.relative(sourcesRootDirectory, file),
    type: severity,
    linenumber: lineno ? parseInt(lineno, 10) : undefined,
    columnnumber: column ? parseInt(column, 10) : undefined,
    code: `TS${code}`
  };
  if (/^warning$/ui.test(severity)) {
    runTracker.warningCount += 1;
  } else if (/^error$/ui.test(severity)) {
    runTracker.errorCount += 1;
  }
  command('task.logissue', tscCmdProps, message);
});

tscRunner.exec({ ignoreReturnCode: true }).then(exitCode => {
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
