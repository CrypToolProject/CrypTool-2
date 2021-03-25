Building MSIEVE for CrypTool 2
===============

To compile this project, you need Visual Studio 2019.
The libraries MPIR and PTHREADS in subdirectories "../mpir" and "../pthreads" need to be compiled first. 
See the information files "README.CT2" in these directories for more information.

After successful compilation of these libraries, the solution "msieve.sln" can be compiled as well.
Depending on the build settings, the compile output will be either the file "msieve.dll" or the file "msieve64.dll".