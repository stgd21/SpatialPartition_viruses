using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace SpatialPartitionPattern
{
    public class GameController : MonoBehaviour
    {
        public int numberOfAntibodies = 1;
        public int numberOfViruses = 1;
        public Text virusText;
        public Slider virusSlider;
        public Text antibodyText;
        public Slider antibodySlider;
        public Button goButton;

        public GameObject friendlyObj;
        public GameObject enemyObj;

        //Change materials to detect which enemy is the closest
        public Material enemyMaterial;
        public Material closestEnemyMaterial;

        //To get a cleaner workspace, parent all soldiers to these empty gameobjects
        public Transform enemyParent;
        public Transform friendlyParent;

        //Store all soldiers in these lists
        List<Soldier> enemySoldiers = new List<Soldier>();
        List<Soldier> friendlySoldiers = new List<Soldier>();

        //Save the closest enemies to easier change back its material
        List<Soldier> closestEnemies = new List<Soldier>();

        //Grid data
        float mapWidth = 50f;
        int cellSize = 10;

        //Number of soldiers on each team
        int numberOfSoldiers = 100;

        //The Spatial Partition grid
        Grid grid;

        bool usePartition = true;
        public Text startText;
        public Text endText;
        public Text differenceText;
        public Toggle usePartitionToggle;
        float startTime;
        float endTime;

        public void UpdateVirusSlider()
        {
            virusText.text = virusSlider.value.ToString();
            numberOfViruses = (int)virusSlider.value;
        }

        public void UpdateAntibodySlider()
        {
            antibodyText.text = antibodySlider.value.ToString();
            numberOfAntibodies = (int)antibodySlider.value;
        }

        public void SpawnEverything()
        {
            goButton.enabled = false;
            goButton.gameObject.SetActive(false);
            //Create a new grid
            grid = new Grid((int)mapWidth, cellSize);

            for (int i = 0; i < numberOfAntibodies; i++)
            {
                //Give the friendly a random position
                Vector3 randomPos = new Vector3(Random.Range(0f, mapWidth), 0.5f, Random.Range(0f, mapWidth));

                //Create a new friendly
                GameObject newFriendly = Instantiate(friendlyObj, randomPos, Quaternion.identity) as GameObject;

                //Add the friendly to a list
                friendlySoldiers.Add(new Friendly(newFriendly, mapWidth));

                //Parent it 
                newFriendly.transform.parent = friendlyParent;
            }

            //Add random enemies and friendly and store them in a list
            for (int i = 0; i < numberOfViruses; i++)
            {
                //Give the enemy a random position
                Vector3 randomPos = new Vector3(Random.Range(0f, mapWidth), 0.5f, Random.Range(0f, mapWidth));

                //Create a new enemy
                GameObject newEnemy = Instantiate(enemyObj, randomPos, Quaternion.identity) as GameObject;

                //Add the enemy to a list
                enemySoldiers.Add(new Enemy(newEnemy, mapWidth, grid));

                //Parent it
                newEnemy.transform.parent = enemyParent;
            }
        }


        void Update()
        {
            startTime = Time.time;
            startText.text = startTime.ToString();

            //Move the enemies
            for (int i = 0; i < enemySoldiers.Count; i++)
            {
                enemySoldiers[i].Move();
            }

            //Reset material of the closest enemies
            for (int i = 0; i < closestEnemies.Count; i++)
            {
                closestEnemies[i].soldierMeshRenderer.material = enemyMaterial;
            }

            //Reset the list with closest enemies
            closestEnemies.Clear();

            //For each friendly, find the closest enemy and change its color and chase it
            for (int i = 0; i < friendlySoldiers.Count; i++)
            {
                Soldier closestEnemy;
                if (usePartition == true)
                {
                    //The fast version with spatial partition
                    closestEnemy = grid.FindClosestEnemy(friendlySoldiers[i]);
                }
                else
                {
                    //The slow version
                    closestEnemy = FindClosestEnemySlow(friendlySoldiers[i]);
                }

                //If we found an enemy
                if (closestEnemy != null)
                {
                    //Change material
                    //closestEnemy.soldierMeshRenderer.material = closestEnemyMaterial;

                    closestEnemies.Add(closestEnemy);

                    //Move the friendly in the direction of the enemy
                    friendlySoldiers[i].Move(closestEnemy);

                    if (Vector3.Distance(friendlySoldiers[i].soldierTrans.position, closestEnemy.soldierTrans.position) < 0.1)
                    {
                        closestEnemy.soldierMeshRenderer.enabled = false;
                    }
                }
            }
            endTime = Time.time;
            endText.text = endTime.ToString();
            differenceText.text = (endTime - startTime).ToString();
        }


        //Find the closest enemy - slow version
        Soldier FindClosestEnemySlow(Soldier soldier)
        {
            Soldier closestEnemy = null;

            float bestDistSqr = Mathf.Infinity;

            //Loop thorugh all enemies
            for (int i = 0; i < enemySoldiers.Count; i++)
            {
                //The distance sqr between the soldier and this enemy
                float distSqr = (soldier.soldierTrans.position - enemySoldiers[i].soldierTrans.position).sqrMagnitude;

                //If this distance is better than the previous best distance, then we have found an enemy that's closer
                if (distSqr < bestDistSqr)
                {
                    bestDistSqr = distSqr;

                    closestEnemy = enemySoldiers[i];
                }
            }

            return closestEnemy;
        }
        public void UpdateToggle()
        {
            usePartition = !usePartition;
        }
    }
}