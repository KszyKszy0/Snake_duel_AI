# Snake_duel_AI
 
This is simple AI bot to play dueling snakes build from scratch.
It's my first time building bigger AI project from scratch. The project is using double DQN for acting in enviroment.
The network layers' sizes are 83 75 20 4. There are 4 outputs because of four actions.
I tried different variations of the agent model.
Currently I am using 9x9 grid + 2 inputs of the distance to the apple for encoding the state. (83 input neurons total)
I am using 128 000 replay buffer with 128 minibatch size, the optimizer is ADAM and discount factor is 0.99.
Relu is my activation function, I've tried leaky Relu too, but it didn't improve anything.
Validation is a map that was set by me to test how snake performs in a bit changed enviroment.
Also the ui is really ugly but I was only focused on the AI part.
The project itself is probably terrible but I'm just happy that I made something from scratch.
I will probably add few things in the future or commit some new models i will think of.