# ExperimentControl

Pin allocation:

	If you want to change the pin allocation, or add one cable, use the App.Config file

	P0.0/USER1: Shutter
	P0.1/USER2: lamp control, connected to IN 1 on the relay
	P0.2: Red lamp control, connected to IN2 on the relay
	P0.3: Intervalometer, connected to IN1 and IN2 on the secondary relay
	P0.4: Traverse dir
	P0.5: Traverse pulse



Files Gestion:

	Generate a folder Date_Experiment in Document/ExperimentResults -if needed change it in App.Config (then everything is in this folder)
	Generate a log.txt
	Read Setting.txt for the Pt Grey Camera, number need to be given as float (even integer must take a .)
	Create a folder TankPictures where all the pictures of the tank will be saved im_date.bmp, idem with FlowVisualisation

Creating a new protocol:
	
	If you want to create a new protocol:
		create a class that inherits AControl
		Implement the methods needed (if you don't want to do one of them create the header of the method and just throw NotImplementedException)
		Add the name of the experiment to the enum in IControl
		Add the new experiment constructor in Form1 constructor
		Edit ExpInfo.html