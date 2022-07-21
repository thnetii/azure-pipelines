const path = require('path');
const process = require('process');
const {
  getVariable,
  setResult,
  TaskResult,
  IssueType,
} = require('azure-pipelines-task-lib');
const { logIssue } = require('azure-pipelines-task-lib/task');
const jshintDefaultReporter = /** @type {JSHintReporterModule} */ (
  // @ts-ignore
  require('jshint/src/reporters/default')
);

const errorRegex = /error/iu;

/** @type {JSHintReporterModule} */
module.exports = {
  reporter(errors, data, opts) {
    const sourcesRootDirectory =
      getVariable('Build.SourcesDirectory') ||
      process.env['Build.SourcesDirectory'] ||
      path.resolve();
    const totalCount = { warning: 0, error: 0 };
    for (const { file, error: details } of errors) {
      const relPath = path.relative(sourcesRootDirectory, file);
      const severity = errorRegex.test(details.id)
        ? IssueType.Error
        : IssueType.Warning;
      logIssue(
        severity,
        details.reason,
        relPath,
        details.line,
        details.character,
        details.code
      );
      if (severity === IssueType.Error) {
        totalCount.error += 1;
      } else {
        totalCount.warning += 1;
      }
    }
    let result = TaskResult.Succeeded;
    if (totalCount.warning) result = TaskResult.SucceededWithIssues;
    if (totalCount.error) result = TaskResult.Failed;
    setResult(result, '');
    jshintDefaultReporter.reporter(errors, data, opts);
  },
};

/**
 * @typedef {Object} JSHintErrorRecord
 * @property {string} file filename
 * @property {import('jshint').LintError} error
 */

/**
 * @typedef {(
 *  errors: JSHintErrorRecord[],
 *  data?: import('jshint').LintData,
 *  opts?: import('jshint').LintOptions
 * ) => void} JSHintReporter
 */

/**
 * @typedef {Object} JSHintReporterModule
 * @property {JSHintReporter} reporter
 */
