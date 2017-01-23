using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public class Snake : MonoBehaviour
{
    // Start point of the snake
    private const float START_X = -17;
    private const float START_Y = 4;

    // Tags of prefabs
    private const string FOOD1 = "Food1";
    private const string FOOD2 = "Food2";
    private const string FOOD3 = "Food3";
    private const string BONUS = "Bonus";
    private const string ENERGY = "Energy";
    private const string OBSTACLE = "Obstacle";
    private const string TAIL = "Tail";

    // Knowledge dictionary
    Dictionary<string, float> knowledgeList = new Dictionary<string, float>();

    // Food prefab
    public GameObject food1Prefab;
    public GameObject food2Prefab;
    public GameObject food3Prefab;
    public GameObject energyPrefab;
    public GameObject bonusPrefab;
    public GameObject obstaclePrefab;
    public List<GameObject> foodPrefabs = new List<GameObject>();

    // Borders
    public Transform borderTop;
    public Transform borderBottom;
    public Transform borderLeft;
    public Transform borderRight;

    // Tail prefab
    public GameObject tailPrefab;

    // Tails list
    private List<Transform> tail = new List<Transform>();

    // Current movement direction
    private Vector2 dir = Vector2.right;

    // Last position of snake's head
    private Vector2 lastPosition;

    // Flags used to check on the next move
    private bool didSnakeAteSomething = false;
    private bool gameOver = false;

    // Object for which we are currently going
    private GameObject objectToGoFor = null;

    // Fields storing time, score and energy values
    private int time = 0;
    private int score = 0;
    private int attempt = 1;
    private int energy = 1000;
    private string formattedTime = "0s";

    //Method drawing labels
    private void OnGUI()
    {
        GUI.Label(new Rect((Screen.width / 8) * 6, 10, 200, 20), "Time: " + formattedTime);
        GUI.Label(new Rect((Screen.width / 8) * 6, 30, 200, 20), "Score: " + score);
        GUI.Label(new Rect((Screen.width / 8) * 6, 50, 200, 20), "Energy: " + (energy / 10) + "/100");
        GUI.Label(new Rect((Screen.width / 8) * 6, 70, 200, 20), "Attempt: " + attempt);

        if (objectToGoFor != null)
        {
            GUI.Label(new Rect((Screen.width / 8) * 6, 150, 200, 20), "Currently going for: ");
            GUI.Label(new Rect((Screen.width / 8) * 6, 170, 200, 20), objectToGoFor.tag + " (x: " + 
                objectToGoFor.transform.position.x + " / y: " + objectToGoFor.transform.position.y + ")");
            GUI.Label(new Rect((Screen.width / 8) * 6, 190, 200, 20), "Value: " + getHardcodedValueByTag(objectToGoFor.tag));
            GUI.Label(new Rect((Screen.width / 8) * 6, 210, 200, 20), "Known value: " + getKnownValueByTag(objectToGoFor.tag));
            GUI.Label(new Rect((Screen.width / 8) * 6, 230, 200, 20), "Distance: " + getDistance(transform.position, objectToGoFor.transform.position).ToString("0.00"));
        }
    }

    // Method called after initialisation
    void Start()
    {
        // Add elements to knowledge list
        knowledgeList.Add(FOOD1, 1);
        knowledgeList.Add(FOOD2, 1);
        knowledgeList.Add(FOOD3, 1);
        knowledgeList.Add(ENERGY, 1);
        knowledgeList.Add(BONUS, 1);
        knowledgeList.Add(OBSTACLE, 1);

        // Add food prefabs to list of foods
        foodPrefabs.Add(food1Prefab);
        foodPrefabs.Add(food2Prefab);
        foodPrefabs.Add(food3Prefab);
        // Spawn obstacles
        Invoke("spawnObstacles", 0);
        // Spawn one food
        Invoke("spawnFood", 0);
        // Spawn one food object every 2s, starting at 0s
        InvokeRepeating("spawnFood", 0, 2);
        // Spawn one bonus object every 30s, starting at 30s
        InvokeRepeating("spawnBonus", 30, 30);
        // Spawn one energy object every 25s, starting at 25s
        InvokeRepeating("spawnEnergy", 25, 25);
        // Invoke snake move every 0.1s
        InvokeRepeating("move", 0.1f, 0.1f);
        // Update time every 1s
        InvokeRepeating("updateTime", 1, 1);
    }

    // The most important method used for moving the snake
    private void move()
    {
        // Save last position of snake's head
        lastPosition = new Vector2(transform.position.x, transform.position.y) - dir;

        // Save current position of snake's head
        Vector2 v = transform.position;

        // If there is no object to go for, calculate a new one
        if (objectToGoFor == null)
        {
            updateObjectToGo();
        }

        // Setting new direction for snake
        // First we are going after the x coordinate
        if (transform.position.x != objectToGoFor.transform.position.x)
        {
            if (transform.position.x < objectToGoFor.transform.position.x)
            {
                dir = Vector2.right;
            }
            else
            {
                dir = -Vector2.right;
            }
        }
        // If x coordinate is correct, we are going for y coordinate
        else
        {
            if (transform.position.y < objectToGoFor.transform.position.y)
            {
                dir = Vector2.up;
            }
            else
            {
                dir = -Vector2.up;
            }
        }

        // Check if snake won't collide with something
        checkMove();

        // Check if snake collided with something or it's energy is empty
        if (gameOver || energy < 1)
        {
            // Set score and energy to initial value
            score = 0;
            attempt++;
            energy = 1000;
            // Rest object to go
            objectToGoFor = null;
            // Remove all tail elements from the game
            foreach (Transform element in tail)
            {
                Destroy(element.gameObject);
            }
            tail.Clear();
            // Delete all energy and bonus objects
            List<GameObject> gameObjectsList = new List<GameObject>();
            gameObjectsList.AddRange(GameObject.FindGameObjectsWithTag(ENERGY));
            gameObjectsList.AddRange(GameObject.FindGameObjectsWithTag(BONUS));
            foreach(GameObject gameObject in gameObjectsList)
            {
                Destroy(gameObject);
            }
            // Delete all food objects except 2 of them
            gameObjectsList.Clear();
            gameObjectsList.AddRange(GameObject.FindGameObjectsWithTag(FOOD1));
            gameObjectsList.AddRange(GameObject.FindGameObjectsWithTag(FOOD2));
            gameObjectsList.AddRange(GameObject.FindGameObjectsWithTag(FOOD3));
            for (int i = 0; i < gameObjectsList.Count - 2; i++)
            {
                Destroy(gameObjectsList[i]);
            }
            // Move snake to the start point
            transform.position = new Vector2(START_X, START_Y);
        }
        else
        {
            // Move snake
            transform.Translate(dir);
        }
        // Reset 'game over' flag
        gameOver = false;

        // Check if snake ate something
        if (didSnakeAteSomething)
        {
            // Reset current object to go
            objectToGoFor = null;
            // Add tail prefab to the game world
            GameObject g = (GameObject)Instantiate(tailPrefab,
                v,
                Quaternion.identity);

            // Add new tail to list
            tail.Insert(0, g.transform);

            // Reset the flag
            didSnakeAteSomething = false;
        }
        // Check if snake has a tail
        else if (tail.Count > 0)
        {
            // Move last element of the tail to where the head was
            tail.Last().position = v;

            // Add tail to the front of list and remove from the back
            tail.Insert(0, tail.Last());
            tail.RemoveAt(tail.Count - 1);
        }

        // Substract energy used for move
        if (energy > 0)
        {
            energy--;
        }
    }

    // Method for checking correctness of the move and adjusting it in case of collisions
    private void checkMove()
    {
        Vector2 currentPosition = new Vector2((int)transform.position.x, (int)transform.position.y);
        Vector2 plannedMove = currentPosition + dir;

        // Find all obstacles on the game world and transform them to vectors
        List<Vector2> colliders = new List<Vector2>();
        List<Vector2> tmpColliders = new List<Vector2>();
        List<GameObject> tmpGO = new List<GameObject>();
        if (getKnownValueByTag(OBSTACLE) == 0)
        {
            tmpGO.AddRange(GameObject.FindGameObjectsWithTag(OBSTACLE));
        }
        tmpGO.AddRange(GameObject.FindGameObjectsWithTag(TAIL));
        colliders = getVectorsFromGameObjects(tmpGO);
        tmpColliders.AddRange(colliders);
        // Remove colliders with distance from snake greater than 1
        foreach (Vector2 collider in tmpColliders)
        {
            if (getDistance(new Vector2(collider.x, collider.y), transform.position) > 1)
            {
                colliders.Remove(collider);
            }
        }

        // Snake doesn't have any tail prefab (and it's collider)
        // so we add a collider of last snake position
        // because snake shouldn't go backwards
        if (colliders.Count == 0)
        {
            colliders.Add(lastPosition);
        }

        // Check if there are any colliders (doesn't make sense to check if move is correct if there are no colliders)
        // and after that check if move is correct
        // (one object will be a tail of snake)
        if (colliders.Count > 0 && !isMoveOk(colliders, plannedMove))
        {
            // Create list of snake's possible moves
            List<Vector2> possibleMoves = new List<Vector2>();
            // Add all 4 possible directions
            possibleMoves.Add(currentPosition + Vector2.right);
            possibleMoves.Add(currentPosition + Vector2.left);
            possibleMoves.Add(currentPosition + Vector2.up);
            possibleMoves.Add(currentPosition + Vector2.down);
            // Remove planned move (it's incorrect)
            deleteVectorFromList(possibleMoves, plannedMove);

            // Delete all moves that are not possible
            foreach (Vector2 collider in colliders)
            {
                deleteVectorFromList(possibleMoves, collider);
            }
            // Check if there are any correct moves left
            if (possibleMoves.Count > 0)
            {
                if (possibleMoves.Count == 1)
                {
                    dir = possibleMoves[0] - currentPosition;
                } else
                {
                    if (objectToGoFor.transform.position.x != currentPosition.x
                        && objectToGoFor.transform.position.y != currentPosition.y)
                    {
                        float distance = float.MaxValue;
                        Vector2 newDirVector = possibleMoves[0];
                        foreach (Vector2 vector in possibleMoves)
                        {
                            float tmpDistance = getDistance(vector, objectToGoFor.transform.position);
                            if (tmpDistance < distance)
                            {
                                distance = tmpDistance;
                                newDirVector = vector;
                            }
                        }
                        dir = newDirVector - currentPosition;
                    } else
                    {
                        // Get new random move
                        dir = possibleMoves[Random.Range(0, possibleMoves.Count)] - currentPosition;
                    }
                }
            }
            else
            {
                // Game over
                print("No moves - game over");
            }
        }
    }

    // Getting vectors of passed game objects
    private List<Vector2> getVectorsFromGameObjects(List<GameObject> gameObjects)
    {
        List<Vector2> vectors = new List<Vector2>();
        foreach(GameObject gameObject in gameObjects)
        {
            vectors.Add(new Vector2(gameObject.transform.position.x, gameObject.transform.position.y));
        }
        return vectors;
    }

    // Method checks if planned move won't collide with any of the present objects
    private bool isMoveOk(List<Vector2> colliders, Vector2 plannedMove)
    {
        // Return false if move collide with walls
        if (plannedMove.x == borderLeft.position.x
            || plannedMove.x == borderRight.position.x
            || plannedMove.y == borderTop.position.y
            || plannedMove.y == borderBottom.position.y)
        {
            return false;
        }

        // Return false if move collide with any of the provided game objects
        foreach (Vector2 collider in colliders)
        {
            if (collider.x == plannedMove.x
                && collider.y == plannedMove.y)
            {
                return false;
            }
        }
        return true;
    }

    // Method deletes vector from provided list
    private void deleteVectorFromList(List<Vector2> vectorsList, Vector2 vector)
    {
        List<Vector2> tmpVectorsList = new List<Vector2>();
        tmpVectorsList.AddRange(vectorsList);
        foreach (Vector2 vect in tmpVectorsList)
        {
            if (vect.x == vector.x && vect.y == vector.y)
            {
                vectorsList.Remove(vect);
            }
        }
    }

    // Method for checking if there is any object present on given coordinates
    private bool checkIfObjectIsPresent(int x, int y)
    {
        List<Transform> allObjects = (GameObject.FindObjectsOfType(typeof(Transform)) as Transform[]).ToList();
        foreach (Transform transform in allObjects)
        {
            if (transform.position.x == x && transform.position.y == y)
            {
                return true;
            }
        }
        return false;
    }

    #region object trigger listener

    // Method triggered when snake collide with object
    void OnTriggerEnter2D(Collider2D coll)
    {
        switch (coll.gameObject.tag)
        {
            case FOOD1:
            case FOOD2:
            case FOOD3:
            case BONUS:
                updateDictionary(coll.gameObject.tag);
                // Set 'ate flag' to true and set new score
                didSnakeAteSomething = true;
                score = score + (int)getHardcodedValueByTag(coll.gameObject.tag);
                // Remove object
                Destroy(coll.gameObject);
                break;
            case ENERGY:
                // Set 'ate flag' and set new energy
                didSnakeAteSomething = true;
                energy = energy + 250;
                // Remove object
                Destroy(coll.gameObject);
                break;
            // Collided with obstacle
            case OBSTACLE:
                // Restart the game
                gameOver = true;
                updateDictionary(coll.gameObject.tag);
                break;
            default:
                // Restart the game
                gameOver = true;
                break;
        }
    }

    // Method for updating known value of object
    private void updateDictionary(string tag)
    {
        if (knowledgeList[tag] == 1)
        {
            knowledgeList[tag] = getHardcodedValueByTag(tag);
        }
    }

    #endregion

    #region best object calculations

    // Method which calculates the best object to get
    private GameObject getBestObject(float startX, float startY, List<ObjectWithValue> objects)
    {
        GameObject bestObject = null;
        float bestValue = float.MaxValue;
        // Return null if there was no objects provided
        if (objects.Count == 0)
        {
            return null;
        }

        // Find the best object from list
        foreach (ObjectWithValue objectWithValue in objects)
        {
            float value = getDistanceValue(startX, startY, objectWithValue);
            if (value != 0 && value < bestValue)
            {
                bestObject = objectWithValue.gameObject;
                bestValue = value;
            }
        }
             
        return bestObject;
    }

    // Method updating current best object to go
    private void updateObjectToGo()
    {
        // Add all eatable objects
        List<GameObject> allObjects = new List<GameObject>();
        allObjects.AddRange(GameObject.FindGameObjectsWithTag(FOOD1));
        allObjects.AddRange(GameObject.FindGameObjectsWithTag(FOOD2));
        allObjects.AddRange(GameObject.FindGameObjectsWithTag(FOOD3));
        allObjects.AddRange(GameObject.FindGameObjectsWithTag(ENERGY));
        allObjects.AddRange(GameObject.FindGameObjectsWithTag(BONUS));
        allObjects.AddRange(GameObject.FindGameObjectsWithTag(OBSTACLE));

        List<ObjectWithValue> allVObjects = new List<ObjectWithValue>();
        foreach (GameObject go in allObjects)
        {
            // Update object values from knowledge
            allVObjects.Add(new ObjectWithValue(go, getKnownValueByTag(go.tag)));
        }

        // Set new object go
        objectToGoFor = getBestObject(transform.position.x, transform.position.y, allVObjects);
    }

    private float getKnownValueByTag(string tag)
    {
        if (tag.Equals(ENERGY))
        {
            return getEnergyValue();
        }
        return knowledgeList[tag];
    }

    // Method for calculating value of the energy object
    private float getEnergyValue()
    {
        if (energy > 850)
        {
            return (1000 - energy) / 1000;
        }
        else
        {
            return (86 - (energy / 10)) / 5;
        }
    }

    // Getting values for objects
    private float getHardcodedValueByTag(string tag)
    {
        float value;
        switch (tag)
        {
            case FOOD1:
                value = 1;
                break;
            case FOOD2:
                value = 2;
                break;
            case FOOD3:
                value = 3;
                break;
            case OBSTACLE:
                value = 0;
                break;
            case BONUS:
                value = 10;
                break;
            case ENERGY:
                value = getEnergyValue();
                break;
            default:
                value = 1;
                break;
        }
        return value;
    }

    #endregion

    #region spawning methods

    // Method for spawning obstacles
    private void spawnObstacles()
    {
        for (int i = 0; i < 100; i++)
        {
            int x, y;
            do
            {
                x = getRandomXValue();
                y = getRandomYValue();
            } while (checkIfObjectIsPresent(x, y));

            Instantiate(obstaclePrefab,
                        new Vector2(x, y),
                        Quaternion.identity);
        }
    }

    // Spawn one piece of food
    void spawnFood()
    {
        int x, y;
        do
        {
            x = getRandomXValue();
            y = getRandomYValue();
        } while (checkIfObjectIsPresent(x, y));

        Instantiate(foodPrefabs[getRandomValue(0, 3)],
                    new Vector2(x, y),
                    Quaternion.identity);

        updateObjectToGo();
    }

    // Spawn one piece of bonus
    void spawnBonus()
    {
        int x, y;
        do
        {
            x = getRandomXValue();
            y = getRandomYValue();
        } while (checkIfObjectIsPresent(x, y));

        Instantiate(bonusPrefab,
                    new Vector2(x, y),
                    Quaternion.identity);

        updateObjectToGo();
    }

    // Spawn one piece of energy
    private void spawnEnergy()
    {
        int x, y;
        do
        {
            x = getRandomXValue();
            y = getRandomYValue();
        } while (checkIfObjectIsPresent(x, y));

        Instantiate(energyPrefab,
                    new Vector2(x, y),
                    Quaternion.identity);

        updateObjectToGo();
    }

    #endregion

    #region math utils

    // Method for updating time and formatting time string which will be later drawn on screen
    private void updateTime()
    {
        time++;
        if (time < 60)
        {
            formattedTime = time + "s";
        }
        else if (time < 60 * 60)
        {
            formattedTime = time / 60 + "m " + (time - time / 60 * 60) + "s";
        }
        else if (time < 60 * 60 * 60)
        {
            int hours = (time / (3600));
            int minutes = ((time - (hours * 3600)) / 60);
            int seconds = time - (hours * 3600) - (minutes * 60);
            formattedTime = hours + "h "
                + minutes + "m "
                + seconds + "s";
        }
    }

    // Method for returning distance between 2 points (x, y)
    private float getDistance(Vector2 point1, Vector2 point2)
    {
        return (Mathf.Pow(point1.x - point2.x, 2) + Mathf.Pow(point1.y - point2.y, 2));
    }

    // Method for getting value (importance) of the object.
    // Value is used to calculate the best object to get.
    private float getDistanceValue(float startX, float startY, ObjectWithValue objectWithValue)
    {
        return (Mathf.Sqrt(Mathf.Pow((objectWithValue.gameObject.transform.position.x - startX), 2)
            + Mathf.Pow((objectWithValue.gameObject.transform.position.y - startY), 2)))
            / objectWithValue.value;
    }

    // Getting random X value from game area
    private int getRandomXValue()
    {
        return getRandomValue(borderLeft.position.x, borderRight.position.x);
    }

    // Getting random Y value from game area
    private int getRandomYValue()
    {
        return getRandomValue(borderTop.position.y, borderBottom.position.y);
    }
    
    // Getting random value between 2 numbers provided
    private int getRandomValue(float point1, float point2)
    {
        return (int)Random.Range(point1, point2);
    }

    #endregion

    #region game object class

    // Class representing spawned objects that can be eaten by snake (food, bonus, energy)
    public class ObjectWithValue
    {
        public GameObject gameObject;
        public float value;

        public ObjectWithValue(GameObject gameObject)
        {
            this.gameObject = gameObject;
            value = 1;
        }

        public ObjectWithValue(GameObject gameObject, float value)
        {
            this.gameObject = gameObject;
            this.value = value;
        }
    }

    #endregion
}