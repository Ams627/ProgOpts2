

if (Get-Command "nuget.exe" -ErrorAction SilentlyContinue) 
{ 
   write-host nuget.exe found
   nuget pack 
}

