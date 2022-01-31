#!/usr/bin/env node
import path from "path";
import { command, getVariable } from "azure-pipelines-task-lib";
import { _writeLine } from "azure-pipelines-task-lib/internal.js";
import { ToolRunner } from "azure-pipelines-task-lib/toolrunner.js";
const argv = process.argv.slice(2);

const sourcesRootDirectory = getVariable("Build.SourcesDirectory")
  || process.env["Build.SourcesDirectory"] || path.resolve();

const eslintRunner = new ToolRunner("npx");
eslintRunner.arg("eslint");
eslintRunner.arg(argv);

/** @see https://github.com/microsoft/vscode/blob/7db1a2b88f7557e0a43fec75b6ba7e50b3e9f77e/src/vs/workbench/contrib/tasks/common/problemMatcher.ts#L1285-L1294 */
const eslintRegexMatcher = /^(.+):\sline\s(\d+),\scol\s(\d+),\s(Error|Warning|Info)\s-\s(.+)\s\((.+)\)$/u;

eslintRunner.on("stdline", /** @param {string} line */ line => {
  _writeLine(line);
  const tscOutputMatch = eslintRegexMatcher.exec(line);
  if (!tscOutputMatch) {
    return;
  }
  const [, file, lineno, column, severity, message, code] = tscOutputMatch;
  const logCmdProps = {
    sourcepath: path.relative(sourcesRootDirectory, file),
    type: severity,
    linenumber: lineno ? parseInt(lineno, 10) : undefined,
    columnnumber: column ? parseInt(column, 10) : undefined,
    code
  };
  command("task.logissue", logCmdProps, message);
});

eslintRunner.exec().then(exitCode => {
  process.exitCode = exitCode;
});
