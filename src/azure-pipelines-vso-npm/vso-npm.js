#!/usr/bin/env node
const os = require('os');
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

/** @type {{props: Object, message: string}[]} */
const logIssueBuffer = [];
/** @type {onMatchCallback} */
const pushLogIssue = (props, message) => {
  logIssueBuffer.push({ props, message });
};

/** @param {string} line */
const onStdErrLine = (line) => {
  if (tryGetErrorMatch(line, pushLogIssue)) {
    runTracker.errorCount += 1;
  } else if (tryGetWarningMatch(line, pushLogIssue)) {
    runTracker.warningCount += 1;
  }
};

/**
 * @typedef {Object} IndexOfAnyNoMatchInfo
 * @property {false} found
 * @property {-1} idx
 */

/**
 * @typedef {Object} IndexOfAnyFoundMatchInfo
 * @property {true} found
 * @property {number} idx
 * @property {string} match
 */

/**
 * @typedef {IndexOfAnyNoMatchInfo | IndexOfAnyFoundMatchInfo} IndexOfAnyInfo
 */

/**
 * @param {string} str
 * @param {string[]} search
 */
const getIndexOfAnyInfo = (str, search) => {
  /** @type {IndexOfAnyInfo} */
  let result = {
    found: false,
    idx: -1,
  };

  /**
   * @param {IndexOfAnyInfo} r1
   * @param {IndexOfAnyFoundMatchInfo} r2
   */
  const getBetterResult = (r1, r2) => {
    if (!r1.found) { return r2; }
    if (r1.idx < r2.idx) { return r1; }
    if (r1.idx > r2.idx) { return r2; }
    if (r1.match.length > r2.match.length) { return r1; }
    return r2;
  };

  for (const srch of search) {
    const idx = str.indexOf(srch);
    if (idx >= 0) {
      result = getBetterResult(result, {
        found: true,
        idx,
        match: srch,
      });
    }
  }
  return result;
};

let stderrBuffer = '';
toolRunner.on('stderr', /** @param {Buffer} data */(data) => {
  /** @see https://docs.microsoft.com/en-us/dotnet/api/system.io.textreader.readline?view=net-6.0#remarks */
  const eol = ['\r', '\n', '\r\n', os.EOL];
  try {
    let s = stderrBuffer + data.toString();
    let idxInfo = getIndexOfAnyInfo(s, eol);
    while (idxInfo.found) {
      const line = s.substring(0, idxInfo.idx);
      onStdErrLine(line);
      // the rest of the string ...
      s = s.substring(idxInfo.idx + idxInfo.match.length);
      idxInfo = getIndexOfAnyInfo(s, eol);
    }
    stderrBuffer = s;
  } catch (err) {
    // streaming lines to console is best effort.  Don't fail a build.
    debug('error processing line');
  }
});

toolRunner.exec({ ignoreReturnCode: true }).then((exitCode) => {
  if (stderrBuffer) {
    onStdErrLine(stderrBuffer);
  }
  return exitCode;
}).then((exitCode) => {
  for (const { props, message } of logIssueBuffer) {
    command('task.logissue', props, message);
  }
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
