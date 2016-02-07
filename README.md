# NNX
Main website: [nnx-addin.org](https://nnx-addin.org).

NNX is a collection of custom functions for Excel for work with simple neural networks. For now, version 0.1 has limited support for defining, training and using multilayer perceptrons, as well as a few utility functions.

# Using NNX
Download and install NNX add-in from the main webstie: [nnx-addin.org](https://nnx-addin.org).

# Building

Prerequisites:
* [Visual Studio 2015](https://www.visualstudio.com/) Community Edition or above. 
* Installer project depends on WiX toolset; download version >= 3.10 from http://wixtoolset.org/.

All other dependencies should be automatically loaded by NuGet during first build. 

Build outputs will go into $(SolutionDir)\NNX\bin\Release. Primary output files:

* NNX-AddIn-packed.xll: add-in for 32 bit Excel. 
* NNX-AddIn64-packed.xll: add-in for 64 bit Excel. 

Ignore all other outputs.

