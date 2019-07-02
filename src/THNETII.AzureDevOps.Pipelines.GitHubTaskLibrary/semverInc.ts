import semver = require("semver");
import fs = require("fs");

const releaseType = <semver.ReleaseType>(process.argv[2]);

fs.readFile("package.json", (error, data) => {
  if (error) {
    console.error("Error reading \"package.json\"", error);
    return;
  }

  let packageObj = JSON.parse(data.toString());
  const originalVersion: string = packageObj.version
  const newVersion = semver.inc(originalVersion, releaseType, { includePrerelease: false });
  if (!newVersion) {
    console.error("Failed to increase original version.", originalVersion);
    return;
  }
  console.log(`version: ${originalVersion} -> ${newVersion}`);
  packageObj.version = newVersion;

  fs.writeFile("package.json", JSON.stringify(packageObj, null, 2), error => {
    if (error) {
      console.error("Error writing \"package.json\"", error);
      return;
    }
  });

  fs.readFile("vss-extension.json", (error, data) => {
    if (error) {
      console.error("Error reading \"vss-extension.json\"", error);
      return;
    }

    let taskObj = JSON.parse(data.toString());
    taskObj.version = newVersion;
    fs.writeFile("vss-extension.json", JSON.stringify(taskObj, null, 2), error => {
      if (error) {
        console.error("Error writing \"vss-extension.json\"", error);
        return;
      }
    });
  });
});
