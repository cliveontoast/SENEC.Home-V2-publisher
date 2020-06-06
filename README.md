
# SENEC.Home-V2-publisher
Task: Display on the cloud, the voltages of the three phases of a house.
i.e.
![Fronius portal](https://raw.githubusercontent.com/cliveontoast/SENEC.Home-V2-publisher/master/end-result.png)

## what it does so far
Runs on a raspeberry pi zero w in Mono
Runs on hardware that supports .net core

Polls local network senec battery webserver and publishes to the cloud.

## todo
Write a web app with react/angular front-end to display the cloud data.


## Notes on installing for raspberry pi

Update the raspberry pi

sudo apt update
sudo apt upgrade
sudo apt install mono-complete
# change the timezone
sudo dpkg-reconfigure tzdata

sudo apt-get install tmux
https://iotpoint.wordpress.com/2016/11/15/tmux-terminal-multiplexer-for-raspberry-pi/

## running cloud.webapp
run `npm install -g @angular/cli`
from cloud.webapp/clientapp folder run `npm install` then run `ng serve`
Run the cloud.webapp from visual studio in IIS express


