#!/usr/bin/env node
const {
  command, debug, setResult, TaskResult,
} = require('azure-pipelines-task-lib');
const { ToolRunner } = require('azure-pipelines-task-lib/toolrunner');

const argv = process.argv.slice(2);

const runTracker = {
  warningCount: 0,
  errorCount: 0,
};
const toolRunner = new ToolRunner('npm');
toolRunner.arg(argv);

/** @see https://github.com/thnetii/gh-actions/blob/f159471f6c222ff68652dca83441d13942100355/src/add-matcher-npm/npm.json */
const regexErrorPattern = '^npm\\s+ERR!\\s+(.*)$';
const regexErrorMatcher = new RegExp(regexErrorPattern, 'u');
const regexWarningPattern = '^npm\\s+WARN\\s+(\\S+)\\s+(.*)$';
const regexWarningMatcher = new RegExp(regexWarningPattern, 'u');

/** @typedef {(props: Object, message: string) => void} onMatchCallback */

/**
 * @param {string} line
 * @param {onMatchCallback} onMatch
 */
const tryGetErrorMatch = (line, onMatch) => {
  const match = regexErrorMatcher.exec(line);
  if (!match) return false;
  const [, message] = match;
  onMatch({ type: 'Error' }, message || '');
  return true;
};

/**
 * @param {string} line
 * @param {onMatchCallback} onMatch
 */
const tryGetWarningMatch = (line, onMatch) => {
  const match = regexWarningMatcher.exec(line);
  if (!match) return false;
  const [, code, message] = match;
  onMatch({ type: 'Warning', code }, message || '');
  return true;
};

/** @type {onMatchCallback} */
const logIssue = (props, message) => {
  command('task.logissue', props, message);
};

toolRunner.on('errline', /** @param {string} line */(line) => {
  if (tryGetErrorMatch(line, logIssue)) {
    runTracker.errorCount += 1;
  } else if (tryGetWarningMatch(line, logIssue)) {
    runTracker.warningCount += 1;
  }
});

toolRunner.exec({ ignoreReturnCode: true }).then((exitCode) => {
  let result = TaskResult.Succeeded;
  if (runTracker.warningCount) {
    result = TaskResult.SucceededWithIssues;
  }
  if (runTracker.errorCount) {
    result = TaskResult.Failed;
  }
  setResult(result, '');
  debug(`npm exited with code '${exitCode}'.`);
});
