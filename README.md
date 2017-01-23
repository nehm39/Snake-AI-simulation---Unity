# Snake AI simulation - Unity

Simple simulation of Snake game written for university assignment.

Simulation board includes 6 kinds of objects: walls, obstacles, energy and 3 types of food.
Snake has its own energy and every move exhaust him. In order to replenish it he has to gather energy object. If he rans out of energy, the simulation ends.
Snake's first job is to learn something about it environment. Initially it doesn't know anything about the objects. 
After learning about environment he goes for the best object on the board (food or energy) using the shortest road.
If snake collide with his tail or an obstacle, simulation restarts.
Main goal of snake is to collect biggest score without dying.

Inside the demo folder there is a build which can be opened using browser and Unity Web Player so it can be tested easily.


![sample](http://i.imgur.com/EcuI1Kv.jpg)
