const fs = require("fs");
const { Octokit } = require("@octokit/rest");

// Read the CHANGELOG.md file
const changelog = fs.readFileSync("CHANGELOG.md", "utf8");

// Extract the section for the latest version
const latestVersion = changelog.split("\n## ")[1];

// Extract the version number from the latest version section
const versionNumber = latestVersion.match(/\d+\.\d+\.\d+/)[0];

// Initialize the Octokit client
const octokit = new Octokit({
    auth: process.env.GREN_GITHUB_TOKEN, // You need to set this environment variable
});

// Check if the release already exists
octokit.repos.getReleaseByTag({
    owner: "Ranoth",
    repo: "MemeBox",
    tag: `v${versionNumber}`,
}).then(() => {
    console.log(`Release v${versionNumber} already exists`);
}).catch(() => {
    // If the release doesn't exist, create it
    octokit.repos.createRelease({
        owner: "Ranoth",
        repo: "MemeBox",
        tag_name: `v${versionNumber}`,
        name: `v${versionNumber}`,
        body: latestVersion,
    }).then(() => {
        console.log(`Created release v${versionNumber}`);
    }).catch((error) => {
        console.error(`Failed to create release: ${error.message}`);
    });
});