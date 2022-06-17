const path = require('path');
const process = require('process');
const {
  getVariable, setResult, TaskResult, IssueType,
} = require('azure-pipelines-task-lib');
const { logIssue } = require('azure-pipelines-task-lib/task');
const { reporter: defaultReporter } = require('jshint/src/reporters/default');

const errorRegex = /error/iu;

/** @type {JSHintReporterModule} */
module.exports = {
  reporter(errors, data, opts) {
    const sourcesRootDirectory = getVariable('Build.SourcesDirectory')
      || process.env['Build.SourcesDirectory'] || path.resolve();
    const totalCount = { warning: 0, error: 0 };
    for (const { file, error: details } of errors) {
      const relPath = path.relative(sourcesRootDirectory, file);
      const severity = errorRegex.test(details.id)
        ? IssueType.Error : IssueType.Warning;
      let message = details.reason;
      if (details.evidence) {
        message = `${message}\n\nCode:\n${details.evidence}`;
      }
      if (details.scope) {
        message = `${message}\n\nin JavaScript scope: ${details.scope}`;
      }
      logIssue(
        severity,
        message,
        relPath,
        details.line,
        details.character,
        details.code,
      );
      if (severity === IssueType.Error) {
        totalCount.error += 1;
      } else { totalCount.warning += 1; }
    }
    let result = TaskResult.Succeeded;
    if (totalCount.warning) result = TaskResult.SucceededWithIssues;
    if (totalCount.error) result = TaskResult.Failed;
    setResult(result, '');
    defaultReporter(errors, data, opts);
  },
};

/**
 * @typedef {Object} JSHintErrorDetail
 * @property {'(error)' | string} id usually '(error)'
 * @property {string} code error/warning code
 * @property {string} reason error/warning message
 * @property {string} evidence a piece of code that generated this error
 * @property {number} line
 * @property {number} character
 * @property {'(main)' | string} scope message scope: usally '(main)' unless the code was eval'ed
 */

/**
 * @typedef {Object} JSHintErrorRecord
 * @property {string} file filename
 * @property {JSHintErrorDetail} error
 */

/**
 * @typedef {(errors: JSHintErrorRecord[], data: *, opts: *) => void} JSHintReporter
 */

/**
 * @typedef {Object} JSHintReporterModule
 * @property {JSHintReporter} reporter
 */
