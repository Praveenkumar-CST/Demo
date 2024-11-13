# my sql connector
import mysql.connector

# Replace these values with your actual database credentials
db_config = {
    'host': 'localhost',            # e.g., 'your-database-host.com' or '127.0.0.1'
    'user': 'root',        # e.g., 'root'
    'password': 'Veeraindhu10',    # e.g., 'password123'
    'database': 'sales'     # e.g., 'my_database'
}

try:
    db_connection = mysql.connector.connect(**db_config)
    cursor = db_connection.cursor()
    print("Connected to the database successfully!")
except mysql.connector.Error as err:
    print(f"Error: {err}")



# Test query (replace 'your_table' with an actual table name in your database)
try:
    query = "SELECT * FROM markets  LIMIT 5;"
    cursor.execute(query)
    results = cursor.fetchall()
    
    # Display the results
    for row in results:
        print(row)
except mysql.connector.Error as err:
    print(f"Query Error: {err}")
    
import google.generativeai as genai
from pathlib import Path
import sqlite3
genai.__version__


# key configuration

genai.configure(api_key = 'AIzaSyC-HCR60M8qHjkQs6-JEDXw_iOjENnYYio')

# Set up the model
generation_config = {
  "temperature": 0.4,
  "top_p": 1,
  "top_k": 32,
  "max_output_tokens": 4096,
}

safety_settings = [
  {
    "category": "HARM_CATEGORY_HARASSMENT",
    "threshold": "BLOCK_MEDIUM_AND_ABOVE"
  },
  {
    "category": "HARM_CATEGORY_HATE_SPEECH",
    "threshold": "BLOCK_MEDIUM_AND_ABOVE"
  },
  {
    "category": "HARM_CATEGORY_SEXUALLY_EXPLICIT",
    "threshold": "BLOCK_MEDIUM_AND_ABOVE"
  },
  {
    "category": "HARM_CATEGORY_DANGEROUS_CONTENT",
    "threshold": "BLOCK_MEDIUM_AND_ABOVE"
  }
]

model = genai.GenerativeModel(model_name = "gemini-pro",
                              generation_config = generation_config,
                              safety_settings = safety_settings)

prompt_parts_1 = [
  "You are an expert in converting English questions to SQL code! The SQL database has the name fashion_products and has the following columns - user_id, product_id, product_name, brand, category, price, color, and size.\n\nFor example,\nExample 1 - How many entries of Adidas are present?, the SQL command will be something like this\n``` SELECT COUNT(*) FROM fashion_products WHERE brand = 'Adidas';\n```\n\nExample 2 - How many XL products of Nike are there that have a rating of more than 4?\n```\nSELECT COUNT(*) FROM fashion_products WHERE brand = 'Nike' AND size = 'XL' AND \"Rating\" > 4;\n```\n\nExample 3 - \n```\nSELECT product_name FROM fashion_products WHERE price = (SELECT MAX(price) FROM fashion_products);\n```\n\nDont include ``` and \\n in the output",
]


# QUESTION 
question = "Tell me the id of the most expensive T-shirt?"


prompt_parts = [prompt_parts_1[0], question]
response = model.generate_content(prompt_parts)
print(response.text)

def generate_gemini_response(question, input_prompt):
    prompt_parts = [input_prompt, question]
    response = model.generate_content(prompt_parts)
    #output = read_sql_query(response.text, "fashion_db.sqlite")
    return response.text

print(generate_gemini_response("How many products of Nike are there?",prompt_parts_1[0]))