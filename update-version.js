const fs = require('fs');
const path = require('path');
const xml2js = require('xml2js');
const parser = new xml2js.Parser();
const builder = new xml2js.Builder();

const packageJson = require('./package.json');
const newVersion = packageJson.version + '.0';

// Update MemeBox.csproj
let csprojPath = path.join(__dirname, 'MemeBox.csproj');
let csprojData = fs.readFileSync(csprojPath, 'utf8');
let csprojResult = csprojData.replace(/<AssemblyVersion>.*<\/AssemblyVersion>/, `<AssemblyVersion>${newVersion}</AssemblyVersion>`);
fs.writeFileSync(csprojPath, csprojResult, 'utf8');

// Update app.xml
let xmlPath = path.join(__dirname, 'app.xml');
fs.readFile(xmlPath, 'utf8', function(err, data) {
    if (err) {
        return console.log(err);
    }
    parser.parseString(data, function(err, result) {
        if (err) {
            return console.log(err);
        }
        result.item.version[0] = newVersion;
        let xml = builder.buildObject(result);
        fs.writeFileSync(xmlPath, xml, 'utf8');
    });
});