# CSC3232 Project - Elementals

### Git Repository: (https://github.com/AnthyClmnt/Elementals/)
### Unity version: 2022.3.1.10f1

## Game Brief 
Elementals is a Player vs Ai turn based game, based on the three major elements: Fire🔥, Water🌊 and Earth🌿. The turn based game has two main objectives:

- Kill enemy characters
- Destroy the enemy shrine

The game will end when one of the shrines has been destroyed

The game is played on a grid map, each tile on the map is either grass, water or rock. 

- Grass can be walked upon by all the elements
- Rock can be walked upon by none of the elements
- Water can be walked upon by water element characters. However its important to note: all character can still attack water elements who are standing on a water tile 

Each character is randomly given 3 attributes.

- ⚔ Attack (how much damage it deals on opponents)
- ❤ Health (how much health the character has)
- 🎯 Range (how many tiles it can reach to move to/attack)

## 🚨 Important

- Please start from then "Menu" scene if not loaded by default
- Please also play the game in QHD (2560 x 1440)

## Controls

### Spawning Characters
- The available characters to spawn are shown to the user below the map grid. Each card will show its type, and its three attribute values.
- To spawn a character, LEFT CLICK on a playing card in your hand to select it, then LEFT CLICK on a valid spawn tile in order to spawn in the character
- A valid spawn tile is one within the first 2 colums of the grid and which doesnt already contain a character

### Moving Characters
- Characters on the board can be moved around the board, this is controlled by their range
- LEFT CLICK on one of your characters on the board, this will highlight all tiles within its range
- Then with the range tiles showing, to move the character RIGHT CLICK on a highlighted tile (within range).
- You cannot move to a tile which already has a character standing on it and the tile must be walkable (refer to game brief)

### Attacking Characters 
- Characters on the board can attack enemies within range
- LEFT CLICK on one of your characters on the board, this will highlight all tiles within its range
- Then with the range tiles showing, LEFT CLICK on a tile within range which contains an enemy character to attack the character
- The health of the enemy characters can be seen on the health bar which is above the character

### Attacking Enemy Shrine ⛩
- Attacking the enemy shrine works in the exact same way as attacking an enemy character
- The selected character must be within range of the shrine, and with the range tiles showing LEFT CLICK on the enemy shrine to attack it
- The shrine helth bar will also show you how much health remains



## Technical Details

### 🌐 Grid Manager
The grid manager handles all the initial grid generation and data storage. The grid is randomly generated each play through, creating a unique enviroment for each game. To ensure a playable game, the grid manager used pathfinding to check if there is at leats a path between the two shrines, if none is found the map is re-generated until a valid path is found.

### 🤖 AI Manager
The Ai manager decides and executes the AI's moves. Once per frame it checks if it the AI's turn. When it is, the ai will Decide on its next more using a range of techniques. It first checks if a pre-determined move must occur, for instance: if the AI has no characters on the board or cards in their hand it will have to end its go. The pre-defined moves prevents unnecessary code execution. 

If no pre-defined moves are chosen, the AI manager will caculate which move to execute. To decide which character to make a move with, it ranks the available characters, firstly by if they are within range of hero characters, then by their playstyle and finally by the current health percentage. The first item in the ordered list is the best character to choose. if this character doesn't have a prefered playStyle, the AI would rather spawn in another character (as long as it has one in their hand).

With the chosen character, the playStyle determines what will be executed. Full information is available in the source code.

### 🚫 Attack Blocks
The game also utilises stochastic behaviour to improve the turn based combat system. Without a blockChance, combat in the game essentially boils down to which of the two characters has a the highest health, the best attack and most imporantly who attacks first. 

The block chance of an attack increased every time either the player or the Ai performs the same attack in a row. This increase also intensifies the longer the repeated attacks occur, up to a maxium of 70% block chance. If a player chooses another charatcer to attack with or attacks a different opponent character, this block chance is reset to 10%.

This randomeness allows for more stategy for both the player and the Ai in which next more is best, it also stops combat being just about who is stronger and who attacks first, by penalising players who continue to perform the same attack. 

### 🎮 PlayStyles
PlayStyles give indiviual AI characters unique movement and decision making. There are 5 playStyles:

- Romaer
    - The romaer playStyle is for characters with very good range. They aim to seek out hero characters on the board 
    - They will (in order):
        - Attack cloest hero character if in range
        - Move towards cloest hero character
        - Attack heroes shrine if in range
        - Move towards heroes shrine
- Defender
    - The Defender playStyle is for characters with very good health. They aim to defend the AI's shrine 
    - They will (in order):
        - Attack weakest hero character within AI's half of the grid
        - Move towards weakest hero character within AI's half of the grid
        - Moving back to their spawn point
- Aggressor
    - The aggressor playStyle is for characters with very good attack damage. Their aim is to avoid hero characters and attack the heroes shrine
    - They will (in order):
        - Move towards heroes shrine
        - Attack weakest character within range
        - Attacking heroes shrine
- Scared
    - The scared playStyle is a bit of a fun one, they are for characters will all bad stats. They do everything possible to avoid getting involved and will run away
        - If they are only scared characters left on board, they are forced to adopt the Defender playStyle. 
- Default
    - The default playStyle is for when none of the criteria for the other playStyles have been met. 
    - They will (in order):
        - Attack hero characters in range
        - Attack heroes shrine if in range
        - Move towards hero shrine 

### 📍 Path Finding

A* pathfinding has been utilised for both hero and AI character movement. This has also been advanced for Aggressor and Default playStyle characters, as these want to avoid hero characters an enemy (E) cost is also considered along with G and H, this E cost we influence the algorithm to avoid tiles near hero characters. Obvsiouly, if there is only one path available the character will still choose this path. 

### 🎯 Range Finding

Range finding uses an algorightm to find the tiles within range from an initial starting position. The function can also ignoreWalkable, this is so the range finder include attack tiles. For instance: if a water element is standing within a water tile, the range finding will still allow for this character to be attacked. 

### ⚙ Difficulty Adjustment

The games difficulty can be adjusted between games. Users can choose between easy, medium and hard. 

- Medium is the default difficulty, where attributes for the character cards are the same for both hero and AI. 
- Easy mode adjusts the characters attributes in favour of the user, allowing for a most statistical probability of the hero having better cards
- Hard mode, does the opposite to easy it adjusts the attributes against the hero, where it becomes more propable of the AI having better characters.
