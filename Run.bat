
//Start one server instance
start cmd.exe @cmd /k " call "Polo Game"\bin\Debug\netcoreapp3.1\Server.exe"

//Set number of client instances to create
SET quantity=2

//Loop start client instances based of parameter
FOR /L %%G IN (1,1,%quantity%) DO start cmd.exe @cmd /k " call Client\bin\Debug\netcoreapp3.1\Client.exe"