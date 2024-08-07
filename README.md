# Kinect Simple Body Tracking CSV Recorder

This is a guideline for using dual Azure Kinect to record body tracking data to a csv file.

Real-time Azure Kinect Body Tracker Viewer is not added yet (needs to be updated before 6/21)

## Hardware and Software Requirements
1. 2 Azure Kinect DK 
1. 1 Host PC (desktop)
1. 1 powered usb hub

## Hardware Prerequisite
1. Required PC :
	- OS : Windows 10(x64)
	- CPU : Quad-core 2.4 GHz or faster processor (8th Generation Intel® Core™ i5 or higher)
	- RAM : 8GB
	- GPU : at least NVIDIA GTX 1070
	- Storage : at least SSD 256GB

## Environment Requirements
![EnvRequirement3](https://github.com/kyungeunvoyage/DualKinect_1M_Collect/assets/86193432/74e60a9e-a5b8-418d-b658-96a3c2d45ffd)
1. We need a space at least 6m*6m (there should be no mirrors in front of each devices)
1. Heights of each Azure Kinects need to be same and at least 1.5m 
1. Dancers should be apart from the Azure kinect at least 1.5m ~ 2m

## Hardware Setup 
1. We follow the multicam-sync-daisychain methodology. (take a look at the image below):
        **Connect Subordinate(Slave) first and Master later**
   	- Subordinate : Device Serial No (000365922612) - Plug in 3.5mm audio cable towards **Sync In**
	- Master : Device Serial No (000379204512) - Plug in 3.5mm audio cable towards **Sync Out**
	
3. Open Two instances of Kinect Viewer and follow this url (https://learn.microsoft.com/ko-kr/azure/kinect-dk/multi-camera-sync)
   **Subordinate should be first connected** -> **Master should be connected lastly**

4. If you want to start recording via exe file, you need to abort kinect viewer instances.

## Steps 
1. Run `kinectSimpleBodyTrackingCSVRecorder.exe`
	- (location : kinectSimpleBodyTrackingCSVRecorder\bin\x64\Debug\net6.0-windows)
1. ![Run](https://github.com/kyungeunvoyage/DualKinect_1M_Collect/assets/86193432/ad1c1602-38bb-4862-a0c1-e3542106fd26)
1. Fill out the name of the dancer 
1. Fill out the song number 
1. Fill out the trial number. 

## Usage

1. Install [Azure kinect Sensor SDK](https://learn.microsoft.com/en-us/azure/kinect-dk/sensor-sdk-download) and [Body Tracking SDK](https://learn.microsoft.com/en-us/azure/Kinect-dk/body-sdk-download).
1. Download this tool. (→ [Release page](https://github.com/Hashory/kinectSimpleBodyTrackingCSVRecorder/releases))
1. Run `kinectSimpleBodyTrackingCSVRecorder.exe`.


## CSV file format

The CSV file contains the following information.  
There are 32 joints in total, each with three coordinates (x, y, z) -> it will exported in euler as well as quaternion .
See [here](https://learn.microsoft.com/en-us/azure/kinect-dk/body-joints#joint-hierarchy) for joint order and more information.  

## Update data 
url : https://drive.google.com/drive/folders/1SHkJU6Iyq7UtwTF2LiLn4aSB8AAbw-o4?usp=sharing 


Also, be careful with the coordinate system. Azure kinect uses [this](https://learn.microsoft.com/en-us/azure/kinect-dk/coordinate-systems#3d-coordinate-systems).

## License

[MIT License](LICENSE.txt)

## Acknowledgements

This project uses some open source code from the following sources:

- [Azure Kinect Sensor SDK](https://github.com/microsoft/Azure-Kinect-Samples/)   
	([MIT License](https://github.com/microsoft/Azure-Kinect-Samples/blob/d87e80a2775413ee65f40943bbb65057e4c41976/LICENSE). Copyright: Microsoft Corporation)
- [AKRecorder](https://github.com/shoda888/AKRecorder)   
  ([MIT License](https://github.com/shoda888/AKRecorder/blob/d5cbe673474b2559640fe4f9cfec40a2eac9693e/LICENSE.txt). Copyright: KoheiShoda)


