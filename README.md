# ExperimentControl

Pin allocation:

	P0.0/USER1: Shutter
	P0.1/USER2: lamp control, connected to IN 1 on the relay
	P0.2: Red lamp control, connected to IN2 on the relay
	P0.3: Intervalometer, connected to IN1 and IN2 on the secondary relay
Files Gestion:

	Generate a folder Date_Experiment in Document/ExperimentResults (then everything is in this folder)
	Generate a log.txt
	Read Setting.txt for the Pt Grey Camera, number need to be given as float (even integer must take a .)
	Create a folder TankPictures zhere all the pictures of the tank will be saved im_date.bmp