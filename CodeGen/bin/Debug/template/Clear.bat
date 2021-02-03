echo off

del .\CommandHandles\output\*.cs /q; 
echo clear-handle

del .\Commands\output\*.cs /q; 
echo clear-\Command

del .\Controllers\output\*.cs /q; 
echo clear-\Controller

del .\Dtos\output\*.cs /q; 
echo clear-Dto

del .\Entities\output\*.cs /q; 
echo clear-Entitie

del .\IRepository\output\*.cs /q; 
echo clear-IRepository

del .\Repository\output\*.cs /q; 
echo clear-Repository

del .\Response\output\*.cs /q; 
echo clear-Response

del .\Validators\output\*.cs /q; 
echo clear-Validator

echo ---------------------------------
echo clear complate
echo ---------------------------------
pause