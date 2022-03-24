#!/usr/bin/env node
import path from 'path';
import {
  command, debug, getVariable, setResult, TaskResult,
} from 'azure-pipelines-task-lib';
import { _writeLine } from 'azure-pipelines-task-lib/internal';
import { ToolRunner } from 'azure-pipelines-task-lib/toolrunner';

const argv = process.argv.slice(2);

const sourcesRootDirectory = getVariable('Build.SourcesDirectory')
  || process.env['Build.SourcesDirectory'] || path.resolve();

const runTracker = {
  warningCount: 0,
  errorCount: 0,
};
const eslintRunner = new ToolRunner('npx');
eslintRunner.arg('eslint');
eslintRunner.arg(argv);

/** @see https://github.com/microsoft/vscode/blob/7db1a2b88f7557e0a43fec75b6ba7e50b3e9f77e/src/vs/workbench/contrib/tasks/common/problemMatcher.ts#L1285-L1294 */
const eslintRegexMatcher = /^(.+):\sline\s(\d+),\scol\s(\d+),\s(Error|Warning|Info)\s-\s(.+)\s\((.+)\)$/u;

eslintRunner.on('stdline', /** @param {string} line */ (line) => {
  _writeLine(line);
  const eslintOutputMatch = eslintRegexMatcher.exec(line);
  if (!eslintOutputMatch) {
    return;
  }
  const [, file, lineno, column, severity, message, code] = eslintOutputMatch;
  const logCmdProps = {
    sourcepath: path.relative(sourcesRootDirectory, file),
    type: severity,
    linenumber: lineno ? parseInt(lineno, 10) : undefined,
    columnnumber: column ? parseInt(column, 10) : undefined,
    code,
  };
  if (/^warning$/ui.test(severity)) {
    runTracker.warningCount += 1;
  } else if (/^error$/ui.test(severity)) {
    runTracker.errorCount += 1;
  }
  command('task.logissue', logCmdProps, message);
});

eslintRunner.exec({ ignoreReturnCode: true }).then((exitCode) => {
  let result = TaskResult.Succeeded;
  if (runTracker.warningCount) {
    result = TaskResult.SucceededWithIssues;
  }
  if (runTracker.errorCount) {
    result = TaskResult.Failed;
  }
  setResult(result, '');
  debug(`ESLint exited with code '${exitCode}'.`);
});
