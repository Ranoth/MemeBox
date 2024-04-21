const fs = require("fs");
const { Octokit } = require("@octokit/rest");

// Read the CHANGELOG.md file
const changelog = fs.readFileSync("CHANGELOG.md", "utf8");

// Extract the section for the latest version
const latestVersion = changelog.split("\n## ")[1];

// Initialize the Octokit client
const octokit = new Octokit({
	auth: process.env.GITHUB_TOKEN, // You need to set this environment variable
});

const versionNumber = latestVersion.match(/\d+\.\d+\.\d+/)[0];

// Use the Octokit client to create or edit a release
octokit.repos.createRelease({
    owner: "Ranoth",
    repo: "MemeBox",
    tag_name: `v${versionNumber}`,
    name: `v${versionNumber}`,
    body: latestVersion,
});
