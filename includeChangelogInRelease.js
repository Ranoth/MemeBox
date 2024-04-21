const fs = require("fs");
const { Octokit } = require("@octokit/rest");
const semver = require("semver");
const { version } = require("os");
require("dotenv").config();

const token = process.env.GITHUB_TOKEN;

// Read the CHANGELOG.md file
const changelog = fs.readFileSync("CHANGELOG.md", "utf8");
// console.log(changelog);

// Split the changelog into lines
const lines = changelog.split("\n");

// Group the lines into version sections
const versions = [];
let currentVersion = [];
lines.forEach((line) => {
	if (line.startsWith("### [")) {
		if (currentVersion.length > 0) {
			versions.push(currentVersion.join("\n"));
		}
		currentVersion = [line];
	} else {
		currentVersion.push(line);
	}
});
// Add the last version section
if (currentVersion.length > 0) {
	versions.push(currentVersion.join("\n"));
}
// console.log(versions);

// Extract and sort the version numbers
const sortedVersions = versions
	.map((version) => {
		const match = version.match(/\[(\d+\.\d+\.\d+)\]/);
		return match ? match[1] : null;
	})
	.filter(Boolean) // remove null values
	.sort(semver.rcompare);
// console.log(sortedVersions);

// Take the latest version number
const versionNumber = sortedVersions[0];
// console.log(versionNumber);

const releaseNotes = versions.find((version) =>
	version.startsWith(`### [${versionNumber}]`)
);
// console.log(releaseNotes);

// Initialize the Octokit client
const octokit = new Octokit({
	auth: token, // You need to set this environment variable
});

// Check if the release already exists
octokit.repos
	.getReleaseByTag({
		owner: "Ranoth",
		repo: "MemeBox",
		tag: `v${versionNumber}`,
	})
	.then((response) => {
		// If the release already exists, update it
		octokit.repos
			.updateRelease({
				owner: "Ranoth",
				repo: "MemeBox",
				release_id: response.data.id,
				body: releaseNotes
			})
			.then(() => {
				console.log(`Updated release v${versionNumber}`);
			})
			.catch((error) => {
				console.error(`Failed to update release: ${error.message}`);
			});
	})
	.catch((error) => {
		if (error.status === 404) {
			// If the release doesn't exist, create it
			octokit.repos
				.createRelease({
					owner: "Ranoth",
					repo: "MemeBox",
					tag_name: `v${versionNumber}`,
					name: `v${versionNumber}`,
					body: releaseNotes
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
