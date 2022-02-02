#!/usr/bin/env node
import path from "path";
import { command, getVariable } from "azure-pipelines-task-lib";
import { _writeLine } from "azure-pipelines-task-lib/internal.js";
import { ToolRunner } from "azure-pipelines-task-lib/toolrunner.js";
const argv = process.argv.slice(2);

const sourcesRootDirectory = getVariable("Build.SourcesDirectory")
  || process.env["Build.SourcesDirectory"] || path.resolve();

const tscRunner = new ToolRunner("npx");
tscRunner.arg("tsc");
tscRunner.arg(argv);

/** @see https://github.com/microsoft/vscode/blob/7db1a2b88f7557e0a43fec75b6ba7e50b3e9f77e/extensions/typescript-language-features/package.json#L1296 */
const tscRegexPattern = "^([^\\s].*)[\\(:](\\d+)[,:](\\d+)(?:\\):\\s+|\\s+-\\s+)(error|warning|info)\\s+TS(\\d+)\\s*:\\s*(.*)$";
const tscRegexMatcher = new RegExp(tscRegexPattern, "u");

tscRunner.on("stdline", /** @param {string} line */ line => {
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
    code
  };
  command("task.logissue", tscCmdProps, message);
});

tscRunner.exec().then(exitCode => {
  process.exitCode = exitCode;
});
