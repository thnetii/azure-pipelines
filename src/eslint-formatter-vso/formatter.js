const path = require('path');
const process = require('process');
const {
  getVariable,
  setResult,
  TaskResult,
  IssueType,
} = require('azure-pipelines-task-lib');
const { logIssue } = require('azure-pipelines-task-lib/task');
/** @type {import('eslint').ESLint.Formatter['format']} */
const stylish = require('eslint-formatter-stylish');

/** @type {{[s in import('eslint').Linter.Severity]: IssueType}} */
const issueType = {
  0: IssueType.Warning,
  1: IssueType.Warning,
  2: IssueType.Error,
};

/** @type {import('eslint').ESLint.Formatter} */
const formatter = {
  format(results) {
    const sourcesRootDirectory =
      getVariable('Build.SourcesDirectory') ||
      process.env['Build.SourcesDirectory'] ||
      path.resolve();
    const totalCount = { warning: 0, error: 0, fatal: 0 };
    for (const {
      filePath,
      messages,
      errorCount,
      fatalErrorCount,
      warningCount,
    } of results) {
      const relPath = path.relative(sourcesRootDirectory, filePath);
      for (const { line, column, severity, message, ruleId } of messages) {
        logIssue(
          issueType[severity],
          message,
          relPath,
          line,
          column,
          ruleId === null ? undefined : ruleId
        );
      }
      totalCount.warning += warningCount;
      totalCount.error += errorCount;
      totalCount.fatal += fatalErrorCount;
    }
    let result = TaskResult.Succeeded;
    if (totalCount.warning) result = TaskResult.SucceededWithIssues;
    if (totalCount.error || totalCount.fatal) result = TaskResult.Failed;
    setResult(result, '');
    return stylish(results);
  },
};

module.exports = formatter.format;
