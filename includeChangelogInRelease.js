const fs = require("fs");
const { Octokit } = require("@octokit/rest");
const semver = require("semver");

// Read the CHANGELOG.md file
const changelog = fs.readFileSync("CHANGELOG.md", "utf8");

// Extract all version sections
const versions = changelog.split("\n## ").slice(1);

// Extract and sort the version numbers
const sortedVersions = versions
	.map((version) => version.match(/\d+\.\d+\.\d+/)[0])
	.sort(semver.rcompare);

// Take the latest version number
const versionNumber = sortedVersions[0];

// Initialize the Octokit client
const octokit = new Octokit({
	auth: process.env.GREN_GITHUB_TOKEN, // You need to set this environment variable
});

// Check if the release already exists
octokit.repos
	.getReleaseByTag({
		owner: "Ranoth",
		repo: "MemeBox",
		tag: `v${versionNumber}`,
	})
	.then((response) => {
		if (response.status === 404) {
			throw new Error("Release not found");
		}
		console.log(`Release v${versionNumber} already exists`);
	})
	.catch((error) => {
		if (error.message === "Release not found") {
			// If the release doesn't exist, create it
			octokit.repos
				.createRelease({
					owner: "Ranoth",
					repo: "MemeBox",
					tag_name: `v${versionNumber}`,
					name: `v${versionNumber}`,
					body: versions.find((version) => version.startsWith(versionNumber)),
				})
				.then(() => {
					console.log(`Created release v${versionNumber}`);
				})
				.catch((error) => {
					console.error(`Failed to create release: ${error.message}`);
				});
		} else {
			console.error(`Failed to get release: ${error.message}`);
		}
	});
