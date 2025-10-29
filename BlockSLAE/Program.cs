// See https://aka.ms/new-console-template for more information

using BlockSLAE;
using BlockSLAE.Testing;

const string basePath = @"F:\projects\BlockSLAU\";

var tester = new ComprehensiveTester(basePath);
tester.RunAllTests();

// var example = new Example();
// example.RunTest();