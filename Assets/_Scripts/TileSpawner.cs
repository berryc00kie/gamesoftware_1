using System.Collections.Generic;
using UnityEngine;

namespace TempleRun
{
    public class TileSpawner : MonoBehaviour
    {
        [SerializeField] private int tileStartCount = 10;
        [SerializeField] private int minimunStraightTiles = 3;
        [SerializeField] private int maximunStraightTiles = 15;
        [SerializeField] private GameObject startingTile;
        [SerializeField] private List<GameObject> turnTiles;

        [System.Serializable]
        public class ObstacleData
        {
            public GameObject obstaclePrefab;
            public float horizontalOffset;
        }

        [SerializeField] private List<ObstacleData> obstacles;

        private Vector3 currentTileLocation = Vector3.zero;
        private Vector3 currentTileDirection = Vector3.forward;
        private GameObject prevTile;
        private List<GameObject> currentTiles;
        private List<GameObject> currentObstacles;

        private void Start()
        {
            currentTiles = new List<GameObject>();
            currentObstacles = new List<GameObject>();
            Random.InitState(System.DateTime.Now.Millisecond);
            for (int i = 0; i < tileStartCount; ++i)
            {
                SpawnTile(startingTile.GetComponent<Tile>());
            }
            SpawnTile(SelectRandomGameObjectFromList(turnTiles).GetComponent<Tile>());
        }

        private void SpawnTile(Tile tile, bool spawnObstacle = false)
        {
            Quaternion newTileRotation = tile.gameObject.transform.rotation * Quaternion.LookRotation(currentTileDirection, Vector3.up);
            prevTile = GameObject.Instantiate(tile.gameObject, currentTileLocation, newTileRotation);
            currentTiles.Add(prevTile);

            if (spawnObstacle) SpawnObstacle();
            if (tile.type == TileType.STRAIGHT)
                currentTileLocation += Vector3.Scale(prevTile.GetComponent<Renderer>().bounds.size, currentTileDirection);
        }

        private void DeletePreviousTiles()
        {
            while (currentTiles.Count != 1)
            {
                GameObject tile = currentTiles[0];
                currentTiles.RemoveAt(0);
                Destroy(tile);
            }
            while (currentObstacles.Count != 0)
            {
                GameObject obstacle = currentObstacles[0];
                currentObstacles.RemoveAt(0);
                Destroy(obstacle);
            }
        }

        public void AddNewDirection(Vector3 direction)
        {
            currentTileDirection = direction;
            DeletePreviousTiles();
            Vector3 tilePlacementScale;
            if (prevTile.GetComponent<Tile>().type == TileType.SIDEWAYS)
            {
                tilePlacementScale = Vector3.Scale(prevTile.GetComponent<Renderer>().bounds.size / 2 + (Vector3.one * startingTile.GetComponent<BoxCollider>().size.z / 2), currentTileDirection);
            }
            else
            {
                tilePlacementScale = Vector3.Scale((prevTile.GetComponent<Renderer>().bounds.size - (Vector3.one * 2)) + (Vector3.one * startingTile.GetComponent<BoxCollider>().size.z / 2), currentTileDirection);
            }
            currentTileLocation += tilePlacementScale;
            int currentPathLength = Random.Range(minimunStraightTiles, maximunStraightTiles);
            for (int i = 0; i < currentPathLength; ++i)
            {
                SpawnTile(startingTile.GetComponent<Tile>(), (i == 0) ? false : true);
            }
            SpawnTile(SelectRandomGameObjectFromList(turnTiles).GetComponent<Tile>(), false);
        }

        private void SpawnObstacle()
        {
            if (Random.value > 0.4f) return;
            ObstacleData obstacleData = SelectRandomObstacleData();
            if (obstacleData == null) return;
            Quaternion newObjectRotation = obstacleData.obstaclePrefab.transform.rotation * Quaternion.LookRotation(currentTileDirection, Vector3.up);
            Vector3 rightVector = Vector3.Cross(currentTileDirection, Vector3.up).normalized;
            Vector3 obstaclePosition = currentTileLocation + rightVector * obstacleData.horizontalOffset;
            GameObject obstacle = Instantiate(obstacleData.obstaclePrefab, obstaclePosition, newObjectRotation);
            currentObstacles.Add(obstacle);
        }

        private ObstacleData SelectRandomObstacleData()
        {
            if (obstacles.Count == 0) return null;
            return obstacles[Random.Range(0, obstacles.Count)];
        }

        private GameObject SelectRandomGameObjectFromList(List<GameObject> list)
        {
            if (list.Count == 0) return null;
            return list[Random.Range(0, list.Count)];
        }
    }
}
