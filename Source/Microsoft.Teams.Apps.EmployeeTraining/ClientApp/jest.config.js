module.exports = {
    roots: ["<rootDir>/src"],
    globals: {
        "ts-jest": {
            tsConfig: "tsconfig.test.json"
        }
    },
    transform: {
        "^.+\\.tsx?$": "ts-jest"
    },
    moduleNameMapper: {
        "\\.(css|less|scss|sass)$": "identity-obj-proxy"
    },
    preset: 'ts-jest',
    testRegex: "(/__tests__/.*|(\\.|/)(test|spec))\\.tsx?$",
    moduleFileExtensions: ["ts", "tsx", "js", "jsx", "json", "node"],
    snapshotSerializers: ["enzyme-to-json/serializer"],
    setupTestFrameworkScriptFile: "<rootDir>/src/jest/setup.js"
}