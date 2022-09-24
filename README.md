
# SENEC.Home-V2-publisher
Inspiration: Display using cloud services for free, the electrical sources and consumption for my house

Cloud.WebApp can be published to Azure App Service to run an Angular app to display the following

![Senec personal portal](https://raw.githubusercontent.com/cliveontoast/SENEC.Home-V2-publisher/master/current-development.png)

![Senec personal portal](https://raw.githubusercontent.com/cliveontoast/SENEC.Home-V2-publisher/master/voltage-development.png)

Inspiration came from fronius solarweb.com
![Fronius portal](https://raw.githubusercontent.com/cliveontoast/SENEC.Home-V2-publisher/master/end-result.png)

## processes that run 
Runs LocalPublisher.Framework on a raspeberry pi zero w in Mono

Does not run most recent version via LocalPublisher.WebApp on hardware that supports .net core. It totally could. 

Polls local network senec battery webserver and publishes to the cloud.

## What would it cost you to run?
The services try to use only free services from Azure as much as possible.

CosmosDB provide a free service up to 5GB of data, so a few years of storage.

App services provide up to 10 free services on the lowest compute plans. If outgoing data exceeds a threshold, it would start costing money.

As of writing (16th June 2020), the developer has not received a bill from this subscription.

## todo
Align LocalPublisherWebApp.Startup and LocalPublisherFramework.Program, so LocalPublisherWebApp can run the current version.. again.

Write Azure infrastructure as a service scripts to 

- Build the source code via Pipelines
- Given an Azure subscription id, script a full environment
-- CosmosDB account
-- Private DNS zone (is this needed for cosmosDB?)
-- App service plan
-- App service

## Notes on installing for raspberry pi

Update the raspberry pi

```
sudo apt update
sudo apt upgrade
sudo apt install mono-complete
# change the timezone
sudo dpkg-reconfigure tzdata

sudo apt-get install tmux

sudo raspi-config
sysetm => boot -> desktop
https://lifehacker.com/how-to-control-a-raspberry-pi-remotely-from-anywhere-in-1792892937
sudo apt-get install realvnc-vnc-server

https://www.techno.tips/dt_articles/100-headless-raspberry-pi-4-with-working-team-viewer/

interface VNC
https://www.realvnc.com/en/connect/download/viewer/

run vncserver
connect, i.e. 192.168.0.131:1

```


https://iotpoint.wordpress.com/2016/11/15/tmux-terminal-multiplexer-for-raspberry-pi/

## running cloud.webapp
run `npm install -g @angular/cli`
from cloud.webapp/clientapp folder run `npm install` then run `ng serve`
Run the cloud.webapp from visual studio in IIS express

## Senec ?
Senec https://senec.com/au is a company that built a battery for home storage of solar power generation. It might have trademarks for the word Senec too.



## Tesla
todo more instructions

```
certmgr -ssl -v https://192.168.91.1
```

## Nuget rebuild lock files
http://blog.ctaggart.com/2019/03/using-nuget-lock-file-for-reproducible.html
```
dotnet nuget locals all --clear
git clean -xfd
git rm **/packages.lock.json -f
dotnet restore LocalPublisher.sln
or
dotnet restore LocalPublisher.sln --force-evaluate
```