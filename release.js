require('dotenv').config();
const { exec } = require('child_process');

const command = `gren release --token=${process.env.GITHUB_TOKEN} --override`;

exec(command, (error, stdout, stderr) => {
  if (error) {
    console.error(`exec error: ${error}`);
    return;
  }
  console.log(`stdout: ${stdout}`);
  console.error(`stderr: ${stderr}`);
});