using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;

namespace MyCook
{
    class Program
    {
        public enum CommandType
        {
            DEFAULT = 0,
            HELP = 1,
            ADD_RECEIPE = 2,
            ADD_INGREDIENT = 3,
            MAKE = 4,
            EXIT = 5
        }

        public static CommandType _CC;

        public static List<string> AvailableCommands = new List<string>();

        public static List<Receipe> KnownReceipes = new List<Receipe>();

        public static List<string> AvailableIngredients = new List<string>();

        public static List<string> MealsInFridge = new List<string>();

        static void Main(string[] args)
        {
            AvailableCommands.Add("/help");

            AvailableCommands.Add("/add_receipe");
            AvailableCommands.Add("/print_receipes");
            AvailableCommands.Add("/delet_receipe");
            
            AvailableCommands.Add("/add_ingredient");
            AvailableCommands.Add("/print_ingredients");
            AvailableCommands.Add("/delet_ingredient");

            AvailableCommands.Add("/make");
            AvailableCommands.Add("/print_meals");
            AvailableCommands.Add("/eat");

            AvailableCommands.Add("/clear");
            AvailableCommands.Add("/exit");       

            _CC = CommandType.DEFAULT;

            Console.WriteLine("Welcome to your personal Virtual Fridge 2.0");

            LoadFiles();

            while (Exec());
                
        }

        static bool Exec()
        {         
            switch (_CC)
            {
                case CommandType.DEFAULT:
                    DefaultUpdate();
                    break;

                case CommandType.HELP:
                    HelpUpdate();
                    break;

                case CommandType.EXIT:
                    return false;

                default:
                    DefaultUpdate();
                    break;
            }

            return true;
        }

        static void DefaultUpdate()
        {           
            CommandCheck();
        }

        static void HelpUpdate()
        {
            Console.WriteLine("\nAll available commands:\n" +
                "\t/help\t=> Display a list of all available commands.\n\n" +

                "\tThere is no need to write the \"\" for all thes commands below.\n\n" +

                "\t/add_receipe \"receipe_name\" \"ingredient_name_1\" \"ingredient_name_2\" ...\t=> Add a receipe in the list of receipes followed by a list of ingredients.\n" +
                "\t/print_receipes\t=> Display the list of all known receipes.\n" +
                "\t/delet_receipe \"receipe_name_1\" \"receipe_name_2\" \"receipe_name_3\" ...\t=> Delet receipes from the list.\n\n" +

                "\t/add_ingredient \"ingredient_name\"\t=> Add an ingredient in the fridge.\n" +         
                "\t/print_ingredients\t=> Display the list of all available ingredients in the fridge.\n" +
                "\t/delet_ingredient \"ingredient_name_1\" \"ingredient_name_2\" \"ingredient_name_3\" ...\t=> Delet ingredients from the fridge.\n\n" +

                "\t/make \"receipe_name\"\t=> Make the receipe Meal with ingredients in the fridge.\n" +
                "\t/print_meals\t=> Display the list of all Meals already made in the fridge.\n" +
                "\t/eat \"meal_name_1\" \"meal_name_2\" \"meal_name_3\" ...\t=> Delet Meals from the fridge.\n\n" +

                "\t/clear\t=> Clear the window.\n" +
                "\t/exit\t=> Close application.\n");

            _CC = CommandType.DEFAULT;
        }

        static void CommandCheck()
        {
            Console.Write("\nI'm listening... /exit to leave.\n>>> ");

            string entry = Console.ReadLine();
            string[] command = entry.ToLower().Split(' '); // String Array of the command split by "space"

            while (!AvailableCommands.Contains(command[0]))
            {
                Console.WriteLine("\nCommand " + command[0] + " unknown. /help for more infos.");
                Console.Write("\nI'm listening... /exit to leave.\n>>> ");
                entry = Console.ReadLine();
                command = entry.ToLower().Split(' '); // String Array of the command split by "space"
            }

            Console.WriteLine();

            List<string> l_command = new List<string>(); // Making a List for copy

            foreach (string s in command)
            {
                l_command.Add(s); // Copy of Array into the List
            }

            if (l_command[0] == "/help")
            {                
                _CC = CommandType.HELP;
            }

            if (l_command[0] == "/add_receipe")
            {
                if (l_command.Count < 3) // Verify if name and at last one ingredient is given
                {
                    Console.WriteLine("The Command " + l_command[0] + " requires a name and at least one ingredient. /help for more infos.");
                    return;
                }
                
                if (checkEmptySpaces(l_command) == true) // Verify if name and/or ingredients are not emptyspaces
                {
                    Console.WriteLine("Please enter a real name for the receipe or the ingredients, not just an \"empty space\". /help for more infos.");
                    return;
                }

                if (checkReceipeDuplicate(l_command[1]) == true) // Verify if receipe already exists
                {
                    Console.WriteLine("The Receipe " + l_command[1] + " already exists. /print_receipes to see what receipes already exist.");
                    return;
                }
                
                Receipe _receipe = new Receipe();

                _receipe.ReceipeName = l_command[1];                

                l_command.RemoveAt(0); // Removing the command name from the List
                l_command.RemoveAt(0); // Removing the Receipe name from the List
                                       // Now l_command contain only ingredients ...

                foreach (string s in l_command)
                {
                    _receipe.IngredientList.Add(s); // Copy of l_command (remain ingredients) into the IngredientList of the Receipe
                }

                /*for (int i = 0; i < l_command.Count; i++)
                {
                    _receipe.IngredientList.Add(l_command[i]);
                }*/

                KnownReceipes.Add(_receipe); // Add the finished receipe into the main program List of Receipes
                SaveFile("KnownReceipes");
            }

            if (l_command[0] == "/print_receipes")
            {
                if (KnownReceipes.Count == 0)
                {
                    Console.WriteLine("Your list of Receipes is empty. Are you eating UberEat only or what ? :p");
                    return;
                }

                printReceipes();
            }

            if (l_command[0] == "/delet_receipe")
            {
                if (l_command.Count < 2)
                {
                    Console.WriteLine("The Command " + l_command[0] + " requires at least one receipe. /help for more infos.");
                    return;
                }

                if (checkEmptySpaces(l_command) == true)
                {
                    Console.WriteLine("Please enter a real name for the receipe, not just an \"empty space\". /help for more infos.");
                    return;
                }

                l_command.RemoveAt(0); // Removing the command name from the List
                                       // Now l_command contain only receipes ...

                List<string> ReceipesNames = new List<string>();

                foreach (Receipe receipe in KnownReceipes)
                {
                    ReceipesNames.Add(receipe.ReceipeName);
                }

                List<string> unknownReceipes = new List<string>();

                foreach (string s in l_command)
                {
                    if (ReceipesNames.Contains(s) == false)
                    {                       
                        unknownReceipes.Add(s);
                    }
                }

                List<string> deletedReceipes = new List<string>();

                foreach (string s in l_command)
                {
                    for (int i = 0; i < KnownReceipes.Count; i++)
                    {
                        if (KnownReceipes[i].ReceipeName == s)
                        {
                            deletedReceipes.Add(s);
                            KnownReceipes.Remove(KnownReceipes[i]);
                        }
                    }                    
                }

                Console.WriteLine("\nReceipe(s) \" " + CombineStrings(deletedReceipes) + " \" is/are deleted.");
                Console.WriteLine("\nCan't delet \" " + CombineStrings(unknownReceipes) + " \" : unknown receipe(s). /print_receipes to see known receipes.");

                SaveFile("KnownReceipes");
            }

            if (l_command[0] == "/add_ingredient")
            {
                if (l_command.Count < 2)
                {
                    Console.WriteLine("The Command " + l_command[0] + " requires at least one ingredient. /help for more infos.");
                    return;
                }

                if (checkEmptySpaces(l_command) == true)
                {
                    Console.WriteLine("Please enter a real name for the ingredients, not just an \"empty space\". /help for more infos.");
                    return;
                }

                string[] ingredientsToAdd = new string[command.Length - 1]; // Making an Array for the ingredients only

                l_command.RemoveAt(0); // Removing the command name from the List
                                       // Now l_command contain only ingredients ...
                
                foreach (string s in l_command)
                {
                    AvailableIngredients.Add(s);
                }

                SaveFile("AvailableIngredients");
            }

            if (l_command[0] == "/print_ingredients")
            {
                if (AvailableIngredients.Count == 0)
                {
                    Console.WriteLine("Your fridge is empty, you must be hungry ! :p");
                    return;
                }

                printIngredients();
            }

            if (l_command[0] == "/delet_ingredient")
            {
                if (l_command.Count < 2)
                {
                    Console.WriteLine("The Command " + l_command[0] + " requires at least one ingredient. /help for more infos.");
                    return;
                }

                if (checkEmptySpaces(l_command) == true)
                {
                    Console.WriteLine("Please enter a real name for the ingredients, not just an \"empty space\". /help for more infos.");
                    return;
                }

                l_command.RemoveAt(0); // Removing the command name from the List
                                       // Now l_command contain only ingredients ...

                List<string> unknownIngredients = new List<string>();

                foreach (string s in l_command)
                {
                    if (AvailableIngredients.Contains(s) == false)
                    {
                        unknownIngredients.Add(s);
                    }
                }

                List<string> deletedIngredients = new List<string>();

                foreach (string s in l_command)
                {
                    for (int i = 0; i < AvailableIngredients.Count; i++)
                    {
                        if (AvailableIngredients[i] == s)
                        {
                            deletedIngredients.Add(s);
                            AvailableIngredients.Remove(AvailableIngredients[i]);
                        }
                    }
                }

                Console.WriteLine("\nIngredient(s) \" " + CombineStrings(deletedIngredients) + " \" is/are deleted.");
                Console.WriteLine("\nCan't delet \" " + CombineStrings(unknownIngredients) + " \" : unknown ingredient(s). /print_ingredients to see available ingredients in fridge.");

                SaveFile("AvailableIngredients");
            }

            if (l_command[0] == "/make")
            {
                if (l_command.Count < 2)
                {
                    Console.WriteLine("The Command " + l_command[0] + " requires a receipe name after \"/make\". /help for more infos.");
                    return;
                }

                if (l_command.Count > 2)
                {
                    Console.WriteLine("The Command " + l_command[0] + " requires ONLY ONE receipe name after \"/make\". /help for more infos.");
                    return;
                }

                if (checkEmptySpaces(l_command) == true)
                {
                    Console.WriteLine("Please enter a real name for the receipe, not just an \"empty space\". /help for more infos.");
                    return;
                }

                bool unknownReceipe = true;
                List<string> requiredIngredients = new List<string>();

                foreach (Receipe r in KnownReceipes)
                {
                    if (r.ReceipeName == l_command[1])
                    {
                        unknownReceipe = false;

                        foreach (string s in r.IngredientList)
                        {
                            requiredIngredients.Add(s);
                        }
                    }
                }

                if (unknownReceipe == true)
                {
                    Console.WriteLine("\nReceipe unknown.");
                    return;
                }

                List<string> MissingIngredients = new List<string>();
                bool missingIngredients = false;

                foreach (string ingredient in requiredIngredients)
                {
                    if (AvailableIngredients.Contains(ingredient) == false)
                    {
                        MissingIngredients.Add(ingredient);
                        missingIngredients = true;
                    }
                }

                if (missingIngredients == true)
                {
                    Console.WriteLine("\nIngredient(s) \" " + CombineStrings(MissingIngredients) + " \" is/are missing in the fridge. Can't make the desired Meal sorry.");
                    return;
                }

                foreach (string ingredient in requiredIngredients)
                {
                    AvailableIngredients.Remove(ingredient);
                }

                SaveFile("AvailableIngredients");

                MealsInFridge.Add(l_command[1]);
                Console.WriteLine("\nThe Meal \" " + l_command[1] + " \" has been made. You can now see it in your fridge ! /help for more infos.");

                SaveFile("MealsInFridge");
            }            

            if (l_command[0] == "/print_meals")
            {
                if (MealsInFridge.Count == 0)
                {
                    Console.WriteLine("There is no Meal made in your fridge, you have to cook ! :p");
                    return;
                }

                printMeals();
            }

            if (l_command[0] == "/eat")
            {
                if (l_command.Count < 2)
                {
                    Console.WriteLine("The Command " + l_command[0] + " requires at least one Meal. /help for more infos.");
                    return;
                }

                if (checkEmptySpaces(l_command) == true)
                {
                    Console.WriteLine("Please enter a real name for the Meals, not just an \"empty space\". /help for more infos.");
                    return;
                }

                l_command.RemoveAt(0); // Removing the command name from the List
                                       // Now l_command contain only Meals ...

                List<string> unknownMeals = new List<string>();

                foreach (string s in l_command)
                {
                    if (MealsInFridge.Contains(s) == false)
                    {
                        unknownMeals.Add(s);
                    }
                }

                List<string> deletedMeals = new List<string>();

                foreach (string s in l_command)
                {
                    for (int i = 0; i < MealsInFridge.Count; i++)
                    {
                        if (MealsInFridge[i] == s)
                        {
                            deletedMeals.Add(s);
                            MealsInFridge.Remove(MealsInFridge[i]);
                        }
                    }
                }

                Console.WriteLine("\nMeal(s) \" " + CombineStrings(deletedMeals) + " \" is/are eaten.");
                Console.WriteLine("\nCan't eat \" " + CombineStrings(unknownMeals) + " \" : unknown Meal(s). /print_meals to see available Meals in fridge.");

                SaveFile("MealsInFridge");
            }

            if (l_command[0] == "/clear")
            {
                Console.Clear();
            }

            if (l_command[0] == "/exit")
            {
                _CC = CommandType.EXIT;
            }
        }

        static void printReceipes()
        {
            Console.WriteLine();

            for (int i = 0; i < KnownReceipes.Count; i++)
            {
                Console.Write("- " + KnownReceipes[i].ReceipeName + " : ");

                for (int j = 0; j < KnownReceipes[i].IngredientList.Count; j++)
                {
                    if (KnownReceipes[i].IngredientList.Count == 1)
                        Console.Write(KnownReceipes[i].IngredientList[j]);
                    else if (j == KnownReceipes[i].IngredientList.Count - 1)
                        Console.Write(KnownReceipes[i].IngredientList[j]);
                    else
                        Console.Write(KnownReceipes[i].IngredientList[j] + ", ");
                }
                Console.WriteLine();
            }
        }

        static void printIngredients()
        {
            Console.WriteLine();

            for (int i = 0; i < AvailableIngredients.Count; i++)
            {
                Console.WriteLine("- " + AvailableIngredients[i]);
            }
            Console.WriteLine();
        }

        static void printMeals()
        {
            Console.WriteLine();

            for (int i = 0; i < MealsInFridge.Count; i++)
            {
                Console.WriteLine("- " + MealsInFridge[i]);
            }
            Console.WriteLine();
        }

        static bool checkEmptySpaces(List<string> list)
        {
            bool result = false;

            foreach (string s in list)
            {
                if (s == " " || s == "")
                {
                    result = true;
                }
            }

            return result;
        }

        static bool checkReceipeDuplicate(string name)
        {
            bool result = false;

            foreach (Receipe r in KnownReceipes)
            {
                if (r.ReceipeName == name)
                {
                    result = true;
                }
            }

            return result;
        }

        static string CombineStrings(List<string> list)
        {
            string result = "";

            foreach (string s in list)
            {
                result = result + s + ", ";
            }

            return result;
        }

        static void SaveFile(string type)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };

            string jsonString = "";
            string fileName = "";

            if (type == "KnownReceipes")
            {
                jsonString = JsonSerializer.Serialize(KnownReceipes, options);
                fileName = "Receipes.json";
            }
            else if (type == "AvailableIngredients")
            {
                jsonString = JsonSerializer.Serialize(AvailableIngredients, options);
                fileName = "Ingredients.json";
            }
            else if (type == "MealsInFridge")
            {
                jsonString = JsonSerializer.Serialize(MealsInFridge, options);
                fileName = "Meals.json";
            }

            string programDirectory = Directory.GetParent(Directory.GetParent(Directory.GetParent(Directory.GetCurrentDirectory()).ToString()).ToString()).ToString();

            File.WriteAllText(Path.Combine(programDirectory, fileName), jsonString);
        }

        static void LoadFiles()
        {
            string jsonString = "";         

            string programDirectory = Directory.GetParent(Directory.GetParent(Directory.GetParent(Directory.GetCurrentDirectory()).ToString()).ToString()).ToString();

            if (File.Exists(Path.Combine(programDirectory, "Receipes.json")))
            {
                jsonString = File.ReadAllText(Path.Combine(programDirectory, "Receipes.json"));

                KnownReceipes = JsonSerializer.Deserialize<List<Receipe>>(jsonString);
            }
            else
            {
                Console.WriteLine("\nNo Receipes file found. You don't have any Receipes yet.");
            }
            
            if (File.Exists(Path.Combine(programDirectory, "Ingredients.json")))
            {
                jsonString = File.ReadAllText(Path.Combine(programDirectory, "Ingredients.json"));

                AvailableIngredients = JsonSerializer.Deserialize<List<string>>(jsonString);
            }
            else
            {
                Console.WriteLine("\nNo Ingredients file found. You don't have any Ingredients in your fridge yet.");
            }

            if (File.Exists(Path.Combine(programDirectory, "Meals.json")))
            {
                jsonString = File.ReadAllText(Path.Combine(programDirectory, "Meals.json"));

                AvailableIngredients = JsonSerializer.Deserialize<List<string>>(jsonString);
            }
            else
            {
                Console.WriteLine("\nNo Meals file found. You don't have any Meals made in your fridge yet.");
            }
        }
    }
}
