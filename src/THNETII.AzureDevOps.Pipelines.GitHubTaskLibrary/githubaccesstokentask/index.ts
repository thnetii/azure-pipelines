import tl = require("azure-pipelines-task-lib/task");

async function run(): Promise<void> {
  try {
    const githubEndpointId = tl.getInput("gitHubConnection", true);
    if (!githubEndpointId) {
      throw new Error("Invalid github connection id.");
    }
    const githubEndpointObject = tl.getEndpointAuthorization(githubEndpointId, false);
    let githubEndpointToken: string | undefined;

    if (!!githubEndpointObject) {
      tl.debug("Endpoint scheme: " + githubEndpointObject.scheme);

      switch (githubEndpointObject.scheme) {
        case "PersonalAccessToken":
          githubEndpointToken = githubEndpointObject.parameters.accessToken;
          break;
        case "Token":
        case "OAuth":
          githubEndpointToken = githubEndpointObject.parameters.AccessToken;
          break;
        default:
          if (githubEndpointObject.scheme) {
            const message = `Invalid GitHub service connection scheme: ${githubEndpointObject.scheme}`;
            throw new Error(message);
          }
          break;
      }
    }

    if (!githubEndpointToken) {
      const message = `Invalid GitHub service endpoint: ${githubEndpointId}.`
      throw new Error(message);
    }

    const variableName = "GitHub.AccessToken";
    tl.setVariable(variableName, githubEndpointToken, true);
    tl.setResult(tl.TaskResult.Succeeded, `GitHub access token assigned to variable $(${variableName}).`, true);
  } catch (error) {
    tl.setResult(tl.TaskResult.Failed, error.message);
  }
}

run();
