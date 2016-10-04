using UnityEngine;
using System.Collections;
using System;
using System.Data;
using Mono.Data.Sqlite;
using System.Collections.Generic;
using UnityEngine.UI;


public class HighScoreManager : MonoBehaviour
{

    // String tells the path to the database
    private string connectionString;

    private List<HighScore> highScores = new List<HighScore>();

    public GameObject scorePrefab;

    public Transform scoreParent;

   
    public int topRanks;

    
    public int saveScores;

    
    public InputField enterName;

    
    public GameObject nameDialog;

    // Use this for initialization
    void Start()
    {
        
        connectionString = "URI=file:" + Application.dataPath + "/HighScoreDB.sqlite";

        CreateTable();

        DeleteExtraScore();

        ShowScores();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) 
        {
            nameDialog.SetActive(!nameDialog.activeSelf);

        }
    }

    
    private void CreateTable()
    {
        //Creates the connection
        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            
            dbConnection.Open();

            
            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string sqlQuery = String.Format("CREATE TABLE if not exists HighScores (PlayerID INTEGER PRIMARY KEY  AUTOINCREMENT  NOT NULL  UNIQUE , Name TEXT NOT NULL , Score INTEGER NOT NULL , Date DATETIME NOT NULL  DEFAULT CURRENT_DATE)");

                dbCmd.CommandText = sqlQuery;

                dbCmd.ExecuteScalar();

                dbConnection.Close();
            }
        }
    }

    
    public void EnterName()
    {

        if (enterName.text != string.Empty) 
        {
            int score = UnityEngine.Random.Range(1, 500); 

            InsertScore(enterName.text, score); 

            enterName.text = string.Empty; 

            ShowScores(); 

        }
    }

    // Inserts  the score into the database
    private void InsertScore(string name, int newScore)
    {
        GetScores(); 

        int hsCount = highScores.Count; 

        if (highScores.Count > 0) 
        {
            HighScore lowestScore = highScores[highScores.Count - 1]; 

            //If the lowest score needs to be replaced
            if (lowestScore != null && saveScores > 0 && highScores.Count >= saveScores && newScore > lowestScore.Score)
            {
                DeleteScore(lowestScore.ID); 

                hsCount--; 
            }
        }
        if (hsCount < saveScores) 
        {
            
            
            using (IDbConnection dbConnection = new SqliteConnection(connectionString))
            {
                
                dbConnection.Open();

               
                using (IDbCommand dbCmd = dbConnection.CreateCommand())
                {
                    
                    string sqlQuery = String.Format("INSERT INTO HighScores(Name,Score) VALUES(\"{0}\",\"{1}\")", name, newScore);

                    dbCmd.CommandText = sqlQuery; 
                    dbCmd.ExecuteScalar(); 
                    dbConnection.Close();


                }
            }
        }
    }

    
    // Gets the scores from the database
    private void GetScores()
    {
        highScores.Clear();

        
        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            
            dbConnection.Open();

            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                
                string sqlQuery = "SELECT * FROM HighScores";

                dbCmd.CommandText = sqlQuery;

                using (IDataReader reader = dbCmd.ExecuteReader())
                {
                    while (reader.Read()) 
                    {
                        
                        highScores.Add(new HighScore(reader.GetInt32(0), reader.GetInt32(2), reader.GetString(1), reader.GetDateTime(3)));
                    }

                   
                    dbConnection.Close();
                    reader.Close();
                }
            }
        }

        highScores.Sort(); 
    }

    
    // Deletes a entry
    private void DeleteScore(int id)
    {
       
        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open(); 

            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                
                string sqlQuery = String.Format("DELETE FROM HighScores WHERE PlayerID = \"{0}\"", id);

                dbCmd.CommandText = sqlQuery;

                dbCmd.ExecuteScalar();

                
                dbConnection.Close();


            }
        }
    }

    
    private void ShowScores()
    {
        GetScores(); 

        foreach (GameObject score in GameObject.FindGameObjectsWithTag("Score"))
        {
            
            Destroy(score);
        }

        for (int i = 0; i < topRanks; i++) 
        {
            if (i <= highScores.Count - 1) 
            {
                GameObject tmpObjec = Instantiate(scorePrefab); 

                HighScore tmpScore = highScores[i]; 

                
            }

        }
    }

    
    // Deletes any extra scores
    private void DeleteExtraScore()
    {
        GetScores(); 

        if (saveScores <= highScores.Count) 
        {
            int deleteCount = highScores.Count - saveScores; 

            highScores.Reverse(); 

            using (IDbConnection dbConnection = new SqliteConnection(connectionString)) 
            {
                dbConnection.Open(); 

                using (IDbCommand dbCmd = dbConnection.CreateCommand()) 
                {
                    for (int i = 0; i < deleteCount; i++) 
                    {
                        
                        string sqlQuery = String.Format("DELETE FROM HighScores WHERE PlayerID = \"{0}\"", highScores[i].ID);

                        
                        dbCmd.CommandText = sqlQuery;

                        dbCmd.ExecuteScalar(); 
                    }

                    dbConnection.Close(); 


                }
            }
        }
    }
}
