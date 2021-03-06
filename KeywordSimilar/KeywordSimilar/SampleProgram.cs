﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

using CsvFiles;


namespace LinqToCsvSample
{
    static class Program
    {

        private static void Main(string[] args)
        {
            CsvFile.UseLambdas = true;
            CsvFile.UseTasks = false;
            CsvFile.FastIndexOfAny = true;


            InMemoryFileWithNoHeader();


            var sw = Stopwatch.StartNew();


            CreateRandomClientsCsvFile();

            QueryClients();


            Console.WriteLine("Program complete in " + sw.ElapsedMilliseconds / 1000.0 + " s.");
            Console.ReadKey();
        }

        private static void InMemoryFileWithNoHeader()
        {
            Console.WriteLine("Test In Memory File With No Header");
            var csvDefinition = new CsvDefinition
                {
                    Header = "ClientId|FirstName|LastName|Occupation|City",
                    FieldSeparator = '|'
                };

            var csvWithNoHeader = new CsvFileReader<Client>(
                new StringReader("1|Pascal|Ganaye|CodeMonkey|Macon\r" +
                                 "2|Mick|Jagger|Singer|Dartford"), csvDefinition
                );
            foreach (var c in csvWithNoHeader)
                Console.WriteLine(string.Format("Client #{0}: {1} ({2} in {3})",
                                                c.ClientId,
                                                c.FirstName + (string.IsNullOrEmpty(c.MiddleName) ? " " : " " + c.MiddleName + " ") + c.LastName,
                                                c.Occupation,
                                                c.City));
        }

        private static void QueryClients()
        {

            Console.WriteLine();
            Console.WriteLine("Clients with JFK initials");


            foreach (var c in from client in CsvFile.Read<Client>("clients.csv")
                              where client.FirstName.StartsWith("J")
                                  && client.MiddleName.StartsWith("F")
                                  && client.LastName.StartsWith("K")
                              select client)
            {
                Console.WriteLine(string.Format("Client #{0}: {1} ({2} in {3})",
                    c.ClientId,
                    c.FirstName + (string.IsNullOrEmpty(c.MiddleName) ? " " : " " + c.MiddleName + " ") + c.LastName,
                    c.Occupation,
                    c.City));
            }
        }


        private static void SortClientsByCity()
        {
            Console.WriteLine();
            Console.WriteLine("Sorting clients into a file for each City");
            int processed = 0;

            var files = new Dictionary<string, CsvFile<Client>>();

            foreach (var c in CsvFile.Read<Client>("clients.csv"))
            {
                processed++;
                if (processed % 1000 == 0)
                    Console.Write(string.Format("\r{0} clients sorted.", processed));

                CsvFile<Client> csvFile;
                if (!files.TryGetValue(c.City ?? "Blank", out csvFile))
                {
                    csvFile = new CsvFile<Client>(c.City);
                    files[c.City ?? "Blank"] = csvFile;
                }
                csvFile.Append(c);

            }
            foreach (var f in files.Values)
                f.Dispose();



            Console.WriteLine();
        }

        private static void Top5BiggestCsvFiles()
        {
            Console.WriteLine();
            Console.WriteLine("Getting 5 biggest CSV files...");
            new DirectoryInfo(Directory.GetCurrentDirectory()).GetFiles("*.csv", SearchOption.AllDirectories)
                .OrderByDescending(fi => fi.Length)
                .Take(5)
                .ToCsv("Top5CsvFiles.csv", new CsvDefinition()
                    {
                        FieldSeparator = '\t',
                        Columns = new String[] { "Name", "Length" }
                    });

            Console.WriteLine(File.ReadAllText("Top5CsvFiles.csv"));

        }

        static Random rnd = new Random(38452847);

        private static void CreateRandomClientsCsvFile()
        {
            Console.WriteLine();
            Console.WriteLine("Creating Random Clients...");

            using (var csvFile = new CsvFile<Client>("clients.csv"))
            {
                for (int i = 0; i < 1000000; i++)
                {
                    var user = Client.RandomClient();
                    user.ClientId = i + 100;
                    csvFile.Append(user);

                    if ((i + 1) % 1000 == 0)
                        Console.Write(string.Format("Writing {0} ({1}/{2})\r", "clients.csv", i + 1, 1000000));
                }
            }
            Console.WriteLine();
        }

        private static void dummy()
        {
            using (var csvFile = new CsvFile<Client>("clients.csv"))
            {
                for (int i = 0; i < 1000000; i++)
                {
                    var user = Client.RandomClient();
                    csvFile.Append(user);
                }
            }

        }
        private static string RandomEntry(string[] entries)
        {
            var entryNo = (int)(Math.Pow(rnd.NextDouble(), 1.5) * entries.Length);
            return entries[entryNo];
        }

        public class Client
        {
            public int ClientId;
            public string FirstName;
            public string MiddleName;
            public string LastName;
            public string Occupation;
            public string City;

            static string[] MaleFirstNames = new string[] { "James", "John", "Robert", "Michael", "William", "David", "Richard", "Charles", "Joseph", "Thomas", "Christopher", "Daniel", "Paul", "Mark", "Donald", "George", "Kenneth", "Steven", "Edward", "Brian", "Ronald", "Anthony", "Kevin", "Jason", "Matthew", "Gary", "Timothy", "Jose", "Larry", "Jeffrey", "Frank", "Scott", "Eric", "Stephen", "Andrew", "Raymond", "Gregory", "Joshua", "Jerry", "Dennis", "Walter", "Patrick", "Peter", "Harold", "Douglas", "Henry", "Carl", "Arthur", "Ryan", "Roger", "Joe", "Juan", "Jack", "Albert", "Jonathan", "Justin", "Terry", "Gerald", "Keith", "Samuel", "Willie", "Ralph", "Lawrence", "Nicholas", "Roy", "Benjamin", "Bruce", "Brandon", "Adam", "Harry", "Fred", "Wayne", "Billy", "Steve", "Louis", "Jeremy", "Aaron", "Randy", "Howard", "Eugene", "Carlos", "Russell", "Bobby", "Victor", "Martin", "Ernest", "Phillip", "Todd", "Jesse", "Craig", "Alan", "Shawn", "Clarence", "Sean", "Philip", "Chris", "Johnny", "Earl", "Jimmy", "Antonio", "Danny", "Bryan", "Tony", "Luis", "Mike", "Stanley", "Leonard", "Nathan", "Dale", "Manuel", "Rodney", "Curtis", "Norman", "Allen", "Marvin", "Vincent", "Glenn", "Jeffery", "Travis", "Jeff", "Chad", "Jacob", "Lee", "Melvin", "Alfred", "Kyle", "Francis", "Bradley", "Jesus", "Herbert", "Frederick", "Ray", "Joel", "Edwin", "Don", "Eddie", "Ricky", "Troy", "Randall", "Barry", "Alexander", "Bernard", "Mario", "Leroy", "Francisco", "Marcus", "Micheal", "Theodore", "Clifford", "Miguel", "Oscar", "Jay", "Jim", "Tom", "Calvin", "Alex", "Jon", "Ronnie", "Bill", "Lloyd", "Tommy", "Leon", "Derek", "Warren", "Darrell", "Jerome", "Floyd", "Leo", "Alvin", "Tim", "Wesley", "Gordon", "Dean", "Greg", "Jorge", "Dustin", "Pedro", "Derrick", "Dan", "Lewis", "Zachary", "Corey", "Herman", "Maurice", "Vernon", "Roberto", "Clyde", "Glen", "Hector", "Shane", "Ricardo", "Sam", "Rick", "Lester", "Brent", "Ramon", "Charlie", "Tyler", "Gilbert", "Gene", "Marc", "Reginald", "Ruben", "Brett", "Angel", "Nathaniel", "Rafael", "Leslie", "Edgar", "Milton", "Raul", "Ben", "Chester", "Cecil", "Duane", "Franklin", "Andre", "Elmer", "Brad", "Gabriel", "Ron", "Mitchell", "Roland", "Arnold", "Harvey", "Jared", "Adrian", "Karl", "Cory", "Claude", "Erik", "Darryl", "Jamie", "Neil", "Jessie", "Christian", "Javier", "Fernando", "Clinton", "Ted", "Mathew", "Tyrone", "Darren", "Lonnie", "Lance", "Cody", "Julio", "Kelly", "Kurt", "Allan", "Nelson", "Guy", "Clayton", "Hugh", "Max", "Dwayne", "Dwight", "Armando", "Felix", "Jimmie", "Everett", "Jordan", "Ian", "Wallace", "Ken", "Bob", "Jaime", "Casey", "Alfredo", "Alberto", "Dave", "Ivan", "Johnnie", "Sidney", "Byron", "Julian", "Isaac", "Morris", "Clifton", "Willard", "Daryl", "Ross", "Virgil", "Andy", "Marshall", "Salvador", "Perry", "Kirk", "Sergio", "Marion", "Tracy", "Seth", "Kent", "Terrance", "Rene", "Eduardo", "Terrence", "Enrique", "Freddie", "Wade", "Austin", "Stuart", "Fredrick", "Arturo", "Alejandro", "Jackie", "Joey", "Nick", "Luther", "Wendell", "Jeremiah", "Evan", "Julius", "Dana", "Donnie", "Otis", "Shannon", "Trevor", "Oliver", "Luke", "Homer", "Gerard", "Doug", "Kenny", "Hubert", "Angelo", "Shaun", "Lyle", "Matt", "Lynn", "Alfonso", "Orlando", "Rex", "Carlton", "Ernesto", "Cameron", "Neal", "Pablo", "Lorenzo", "Omar", "Wilbur", "Blake", "Grant", "Horace", "Roderick", "Kerry", "Abraham", "Willis", "Rickey", "Jean", "Ira", "Andres", "Cesar", "Johnathan", "Malcolm", "Rudolph", "Damon", "Kelvin", "Rudy", "Preston", "Alton", "Archie", "Marco", "Wm", "Pete", "Randolph", "Garry", "Geoffrey", "Jonathon", "Felipe", "Bennie", "Gerardo", "Ed", "Dominic", "Robin", "Loren", "Delbert", "Colin", "Guillermo", "Earnest", "Lucas", "Benny", "Noel", "Spencer", "Rodolfo", "Myron", "Edmund", "Garrett", "Salvatore", "Cedric", "Lowell", "Gregg", "Sherman", "Wilson", "Devin", "Sylvester", "Kim", "Roosevelt", "Israel", "Jermaine", "Forrest", "Wilbert", "Leland", "Simon", "Guadalupe", "Clark", "Irving", "Carroll", "Bryant", "Owen", "Rufus", "Woodrow", "Sammy", "Kristopher", "Mack", "Levi", "Marcos", "Gustavo", "Jake", "Lionel", "Marty", "Taylor", "Ellis", "Dallas", "Gilberto", "Clint", "Nicolas", "Laurence", "Ismael", "Orville", "Drew", "Jody", "Ervin", "Dewey", "Al", "Wilfred", "Josh", "Hugo", "Ignacio", "Caleb", "Tomas", "Sheldon", "Erick", "Frankie", "Stewart", "Doyle", "Darrel", "Rogelio", "Terence", "Santiago", "Alonzo", "Elias", "Bert", "Elbert", "Ramiro", "Conrad", "Pat", "Noah", "Grady", "Phil", "Cornelius", "Lamar", "Rolando", "Clay", "Percy", "Dexter", "Bradford", "Merle", "Darin", "Amos", "Terrell", "Moses", "Irvin", "Saul", "Roman", "Darnell", "Randal", "Tommie", "Timmy", "Darrin", "Winston", "Brendan", "Toby", "Van", "Abel", "Dominick", "Boyd", "Courtney", "Jan", "Emilio", "Elijah", "Cary", "Domingo", "Santos", "Aubrey", "Emmett", "Marlon", "Emanuel", "Jerald", "Edmond", "Emil", "Dewayne", "Will", "Otto", "Teddy", "Reynaldo", "Bret", "Morgan", "Jess", "Trent", "Humberto", "Emmanuel", "Stephan", "Louie", "Vicente", "Lamont", "Stacy", "Garland", "Miles", "Micah", "Efrain", "Billie", "Logan", "Heath", "Rodger", "Harley", "Demetrius", "Ethan", "Eldon", "Rocky", "Pierre", "Junior", "Freddy", "Eli", "Bryce", "Antoine", "Robbie", "Kendall", "Royce", "Sterling", "Mickey", "Chase", "Grover", "Elton", "Cleveland", "Dylan", "Chuck", "Damian", "Reuben", "Stan", "August", "Leonardo", "Jasper", "Russel", "Erwin", "Benito", "Hans", "Monte", "Blaine", "Ernie", "Curt", "Quentin", "Agustin", "Murray", "Jamal", "Devon", "Adolfo", "Harrison", "Tyson", "Burton", "Brady", "Elliott", "Wilfredo", "Bart", "Jarrod", "Vance", "Denis", "Damien", "Joaquin", "Harlan", "Desmond", "Elliot", "Darwin", "Ashley", "Gregorio", "Buddy", "Xavier", "Kermit", "Roscoe", "Esteban", "Anton", "Solomon", "Scotty", "Norbert", "Elvin", "Williams", "Nolan", "Carey", "Rod", "Quinton", "Hal", "Brain", "Rob", "Elwood", "Kendrick", "Darius", "Moises", "Son", "Marlin", "Fidel", "Thaddeus", "Cliff", "Marcel", "Ali", "Jackson", "Raphael", "Bryon", "Armand", "Alvaro", "Jeffry", "Dane", "Joesph", "Thurman", "Ned", "Sammie", "Rusty", "Michel", "Monty", "Rory", "Fabian", "Reggie", "Mason", "Graham", "Kris", "Isaiah", "Vaughn", "Gus", "Avery", "Loyd", "Diego", "Alexis", "Adolph", "Norris", "Millard", "Rocco", "Gonzalo", "Derick", "Rodrigo", "Gerry", "Stacey", "Carmen", "Wiley", "Rigoberto", "Alphonso", "Ty", "Shelby", "Rickie", "Noe", "Vern", "Bobbie", "Reed", "Jefferson", "Elvis", "Bernardo", "Mauricio", "Hiram", "Donovan", "Basil", "Riley", "Ollie", "Nickolas", "Maynard", "Scot", "Vince", "Quincy", "Eddy", "Sebastian", "Federico", "Ulysses", "Heriberto", "Donnell", "Cole", "Denny", "Davis", "Gavin", "Emery", "Ward", "Romeo", "Jayson", "Dion", "Dante", "Clement", "Coy", "Odell", "Maxwell", "Jarvis", "Bruno", "Issac", "Pascal" };
            static string[] FemaleFirstNames = new string[] { "Mary", "Patricia", "Linda", "Barbara", "Elizabeth", "Jennifer", "Maria", "Susan", "Margaret", "Dorothy", "Lisa", "Nancy", "Karen", "Betty", "Helen", "Sandra", "Donna", "Carol", "Ruth", "Sharon", "Michelle", "Laura", "Sarah", "Kimberly", "Deborah", "Jessica", "Shirley", "Cynthia", "Angela", "Melissa", "Brenda", "Amy", "Anna", "Rebecca", "Virginia", "Kathleen", "Pamela", "Martha", "Debra", "Amanda", "Stephanie", "Carolyn", "Christine", "Marie", "Janet", "Catherine", "Frances", "Ann", "Joyce", "Diane", "Alice", "Julie", "Heather", "Teresa", "Doris", "Gloria", "Evelyn", "Jean", "Cheryl", "Mildred", "Katherine", "Joan", "Ashley", "Judith", "Rose", "Janice", "Kelly", "Nicole", "Judy", "Christina", "Kathy", "Theresa", "Beverly", "Denise", "Tammy", "Irene", "Jane", "Lori", "Rachel", "Marilyn", "Andrea", "Kathryn", "Louise", "Sara", "Anne", "Jacqueline", "Wanda", "Bonnie", "Julia", "Ruby", "Lois", "Tina", "Phyllis", "Norma", "Paula", "Diana", "Annie", "Lillian", "Emily", "Robin", "Peggy", "Crystal", "Gladys", "Rita", "Dawn", "Connie", "Florence", "Tracy", "Edna", "Tiffany", "Carmen", "Rosa", "Cindy", "Grace", "Wendy", "Victoria", "Edith", "Kim", "Sherry", "Sylvia", "Josephine", "Thelma", "Shannon", "Sheila", "Ethel", "Ellen", "Elaine", "Marjorie", "Carrie", "Charlotte", "Monica", "Esther", "Pauline", "Emma", "Juanita", "Anita", "Rhonda", "Hazel", "Amber", "Eva", "Debbie", "April", "Leslie", "Clara", "Lucille", "Jamie", "Joanne", "Eleanor", "Valerie", "Danielle", "Megan", "Alicia", "Suzanne", "Michele", "Gail", "Bertha", "Darlene", "Veronica", "Jill", "Erin", "Geraldine", "Lauren", "Cathy", "Joann", "Lorraine", "Lynn", "Sally", "Regina", "Erica", "Beatrice", "Dolores", "Bernice", "Audrey", "Yvonne", "Annette", "June", "Samantha", "Marion", "Dana", "Stacy", "Ana", "Renee", "Ida", "Vivian", "Roberta", "Holly", "Brittany", "Melanie", "Loretta", "Yolanda", "Jeanette", "Laurie", "Katie", "Kristen", "Vanessa", "Alma", "Sue", "Elsie", "Beth", "Jeanne", "Vicki", "Carla", "Tara", "Rosemary", "Eileen", "Terri", "Gertrude", "Lucy", "Tonya", "Ella", "Stacey", "Wilma", "Gina", "Kristin", "Jessie", "Natalie", "Agnes", "Vera", "Willie", "Charlene", "Bessie", "Delores", "Melinda", "Pearl", "Arlene", "Maureen", "Colleen", "Allison", "Tamara", "Joy", "Georgia", "Constance", "Lillie", "Claudia", "Jackie", "Marcia", "Tanya", "Nellie", "Minnie", "Marlene", "Heidi", "Glenda", "Lydia", "Viola", "Courtney", "Marian", "Stella", "Caroline", "Dora", "Jo", "Vickie", "Mattie", "Terry", "Maxine", "Irma", "Mabel", "Marsha", "Myrtle", "Lena", "Christy", "Deanna", "Patsy", "Hilda", "Gwendolyn", "Jennie", "Nora", "Margie", "Nina", "Cassandra", "Leah", "Penny", "Kay", "Priscilla", "Naomi", "Carole", "Brandy", "Olga", "Billie", "Dianne", "Tracey", "Leona", "Jenny", "Felicia", "Sonia", "Miriam", "Velma", "Becky", "Bobbie", "Violet", "Kristina", "Toni", "Misty", "Mae", "Shelly", "Daisy", "Ramona", "Sherri", "Erika", "Katrina", "Claire", "Lindsey", "Lindsay", "Geneva", "Guadalupe", "Belinda", "Margarita", "Sheryl", "Cora", "Faye", "Ada", "Natasha", "Sabrina", "Isabel", "Marguerite", "Hattie", "Harriet", "Molly", "Cecilia", "Kristi", "Brandi", "Blanche", "Sandy", "Rosie", "Joanna", "Iris", "Eunice", "Angie", "Inez", "Lynda", "Madeline", "Amelia", "Alberta", "Genevieve", "Monique", "Jodi", "Janie", "Maggie", "Kayla", "Sonya", "Jan", "Lee", "Kristine", "Candace", "Fannie", "Maryann", "Opal", "Alison", "Yvette", "Melody", "Luz", "Susie", "Olivia", "Flora", "Shelley", "Kristy", "Mamie", "Lula", "Lola", "Verna", "Beulah", "Antoinette", "Candice", "Juana", "Jeannette", "Pam", "Kelli", "Hannah", "Whitney", "Bridget", "Karla", "Celia", "Latoya", "Patty", "Shelia", "Gayle", "Della", "Vicky", "Lynne", "Sheri", "Marianne", "Kara", "Jacquelyn", "Erma", "Blanca", "Myra", "Leticia", "Pat", "Krista", "Roxanne", "Angelica", "Johnnie", "Robyn", "Francis", "Adrienne", "Rosalie", "Alexandra", "Brooke", "Bethany", "Sadie", "Bernadette", "Traci", "Jody", "Kendra", "Jasmine", "Nichole", "Rachael", "Chelsea", "Mable", "Ernestine", "Muriel", "Marcella", "Elena", "Krystal", "Angelina", "Nadine", "Kari", "Estelle", "Dianna", "Paulette", "Lora", "Mona", "Doreen", "Rosemarie", "Angel", "Desiree", "Antonia", "Hope", "Ginger", "Janis", "Betsy", "Christie", "Freda", "Mercedes", "Meredith", "Lynette", "Teri", "Cristina", "Eula", "Leigh", "Meghan", "Sophia", "Eloise", "Rochelle", "Gretchen", "Cecelia", "Raquel", "Henrietta", "Alyssa", "Jana", "Kelley", "Gwen", "Kerry", "Jenna", "Tricia", "Laverne", "Olive", "Alexis", "Tasha", "Silvia", "Elvira", "Casey", "Delia", "Sophie", "Kate", "Patti", "Lorena", "Kellie", "Sonja", "Lila", "Lana", "Darla", "May", "Mindy", "Essie", "Mandy", "Lorene", "Elsa", "Josefina", "Jeannie", "Miranda", "Dixie", "Lucia", "Marta", "Faith", "Lela", "Johanna", "Shari", "Camille", "Tami", "Shawna", "Elisa", "Ebony", "Melba", "Ora", "Nettie", "Tabitha", "Ollie", "Jaime", "Winifred", "Kristie", "Marina", "Alisha", "Aimee", "Rena", "Myrna", "Marla", "Tammie", "Latasha", "Bonita", "Patrice", "Ronda", "Sherrie", "Addie", "Francine", "Deloris", "Stacie", "Adriana", "Cheri", "Shelby", "Abigail", "Celeste", "Jewel", "Cara", "Adele", "Rebekah", "Lucinda", "Dorthy", "Chris", "Effie", "Trina", "Reba", "Shawn", "Sallie", "Aurora", "Lenora", "Etta", "Lottie", "Kerri", "Trisha", "Nikki", "Estella", "Francisca", "Josie", "Tracie", "Marissa", "Karin", "Brittney", "Janelle", "Lourdes", "Laurel", "Helene", "Fern", "Elva", "Corinne", "Kelsey", "Ina", "Bettie", "Elisabeth", "Aida", "Caitlin", "Ingrid", "Iva", "Eugenia", "Christa", "Goldie", "Cassie", "Maude", "Jenifer", "Therese", "Frankie", "Dena", "Lorna", "Janette", "Latonya", "Candy", "Morgan", "Consuelo", "Tamika", "Rosetta", "Debora", "Cherie", "Polly", "Dina", "Jewell", "Fay", "Jillian", "Dorothea", "Nell", "Trudy", "Esperanza", "Patrica", "Kimberley", "Shanna", "Helena", "Carolina", "Cleo", "Stefanie", "Rosario", "Ola", "Janine", "Mollie", "Lupe", "Alisa", "Lou", "Maribel", "Susanne", "Bette", "Susana", "Elise", "Cecile", "Isabelle", "Lesley", "Jocelyn", "Paige", "Joni", "Rachelle", "Leola", "Daphne", "Alta", "Ester", "Petra", "Graciela", "Imogene", "Jolene", "Keisha", "Lacey", "Glenna", "Gabriela", "Keri", "Ursula", "Lizzie", "Kirsten", "Shana", "Adeline", "Mayra", "Jayne", "Jaclyn", "Gracie", "Sondra", "Carmela", "Marisa", "Rosalind", "Charity", "Tonia", "Beatriz", "Marisol", "Clarice", "Jeanine", "Sheena", "Angeline", "Frieda", "Lily", "Robbie", "Shauna", "Millie", "Claudette", "Cathleen", "Angelia", "Gabrielle", "Autumn", "Katharine", "Summer", "Jodie", "Staci", "Lea", "Christi", "Jimmie", "Justine", "Elma", "Luella", "Margret", "Dominique", "Socorro", "Rene", "Martina", "Margo", "Mavis", "Callie", "Bobbi", "Maritza", "Lucile", "Leanne", "Jeannine", "Deana", "Aileen", "Lorie", "Ladonna", "Willa", "Manuela", "Gale", "Selma", "Dolly", "Sybil", "Abby", "Lara", "Dale", "Ivy", "Dee", "Winnie", "Leia" };
            static string[] LastNames = new string[] { "Smith", "Johnson", "Williams", "Jones", "Brown", "Davis", "Miller", "Wilson", "Moore", "Taylor", "Anderson", "Thomas", "Jackson", "White", "Harris", "Martin", "Thompson", "Garcia", "Martinez", "Robinson", "Clark", "Rodriguez", "Lewis", "Lee", "Walker", "Hall", "Allen", "Young", "Hernandez", "King", "Wright", "Lopez", "Hill", "Scott", "Green", "Adams", "Baker", "Gonzalez", "Nelson", "Carter", "Mitchell", "Perez", "Roberts", "Turner", "Phillips", "Campbell", "Parker", "Evans", "Edwards", "Collins", "Stewart", "Sanchez", "Morris", "Rogers", "Reed", "Cook", "Morgan", "Bell", "Murphy", "Bailey", "Rivera", "Cooper", "Richardson", "Cox", "Howard", "Ward", "Torres", "Peterson", "Gray", "Ramirez", "James", "Watson", "Brooks", "Kelly", "Sanders", "Price", "Bennett", "Wood", "Barnes", "Ross", "Henderson", "Coleman", "Jenkins", "Perry", "Powell", "Long", "Patterson", "Hughes", "Flores", "Washington", "Butler", "Simmons", "Foster", "Gonzales", "Bryant", "Alexander", "Russell", "Griffin", "Diaz", "Hayes", "Myers", "Ford", "Hamilton", "Graham", "Sullivan", "Wallace", "Woods", "Cole", "West", "Jordan", "Owens", "Reynolds", "Fisher", "Ellis", "Harrison", "Gibson", "Mcdonald", "Cruz", "Marshall", "Ortiz", "Gomez", "Murray", "Freeman", "Wells", "Webb", "Simpson", "Stevens", "Tucker", "Porter", "Hunter", "Hicks", "Crawford", "Henry", "Boyd", "Mason", "Morales", "Kennedy", "Warren", "Dixon", "Ramos", "Reyes", "Burns", "Gordon", "Shaw", "Holmes", "Rice", "Robertson", "Hunt", "Black", "Daniels", "Palmer", "Mills", "Nichols", "Grant", "Knight", "Ferguson", "Rose", "Stone", "Hawkins", "Dunn", "Perkins", "Hudson", "Spencer", "Gardner", "Stephens", "Payne", "Pierce", "Berry", "Matthews", "Arnold", "Wagner", "Willis", "Ray", "Watkins", "Olson", "Carroll", "Duncan", "Snyder", "Hart", "Cunningham", "Bradley", "Lane", "Andrews", "Ruiz", "Harper", "Fox", "Riley", "Armstrong", "Carpenter", "Weaver", "Greene", "Lawrence", "Elliott", "Chavez", "Sims", "Austin", "Peters", "Kelley", "Franklin", "Lawson", "Fields", "Gutierrez", "Ryan", "Schmidt", "Carr", "Vasquez", "Castillo", "Wheeler", "Chapman", "Oliver", "Montgomery", "Richards", "Williamson", "Johnston", "Banks", "Meyer", "Bishop", "Mccoy", "Howell", "Alvarez", "Morrison", "Hansen", "Fernandez", "Garza", "Harvey", "Little", "Burton", "Stanley", "Nguyen", "George", "Jacobs", "Reid", "Kim", "Fuller", "Lynch", "Dean", "Gilbert", "Garrett", "Romero", "Welch", "Larson", "Frazier", "Burke", "Hanson", "Day", "Mendoza", "Moreno", "Bowman", "Medina", "Fowler", "Brewer", "Hoffman", "Carlson", "Silva", "Pearson", "Holland", "Douglas", "Fleming", "Jensen", "Vargas", "Byrd", "Davidson", "Hopkins", "May", "Terry", "Herrera", "Wade", "Soto", "Walters", "Curtis", "Neal", "Caldwell", "Lowe", "Jennings", "Barnett", "Graves", "Jimenez", "Horton", "Shelton", "Barrett", "Obrien", "Castro", "Sutton", "Gregory", "Mckinney", "Lucas", "Miles", "Craig", "Rodriquez", "Chambers", "Holt", "Lambert", "Fletcher", "Watts", "Bates", "Hale", "Rhodes", "Pena", "Beck", "Newman", "Haynes", "Mcdaniel", "Mendez", "Bush", "Vaughn", "Parks", "Dawson", "Santiago", "Norris", "Hardy", "Love", "Steele", "Curry", "Powers", "Schultz", "Barker", "Guzman", "Page", "Munoz", "Ball", "Keller", "Chandler", "Weber", "Leonard", "Walsh", "Lyons", "Ramsey", "Wolfe", "Schneider", "Mullins", "Benson", "Sharp", "Bowen", "Daniel", "Barber", "Cummings", "Hines", "Baldwin", "Griffith", "Valdez", "Hubbard", "Salazar", "Reeves", "Warner", "Stevenson", "Burgess", "Santos", "Tate", "Cross", "Garner", "Mann", "Mack", "Moss", "Thornton", "Dennis", "Mcgee", "Farmer", "Delgado", "Aguilar", "Vega", "Glover", "Manning", "Cohen", "Harmon", "Rodgers", "Robbins", "Newton", "Todd", "Blair", "Higgins", "Ingram", "Reese", "Cannon", "Strickland", "Townsend", "Potter", "Goodwin", "Walton", "Rowe", "Hampton", "Ortega", "Patton", "Swanson", "Joseph", "Francis", "Goodman", "Maldonado", "Yates", "Becker", "Erickson", "Hodges", "Rios", "Conner", "Adkins", "Webster", "Norman", "Malone", "Hammond", "Flowers", "Cobb", "Moody", "Quinn", "Blake", "Maxwell", "Pope", "Floyd", "Osborne", "Paul", "Mccarthy", "Guerrero", "Lindsey", "Estrada", "Sandoval", "Gibbs", "Tyler", "Gross", "Fitzgerald", "Stokes", "Doyle", "Sherman", "Saunders", "Wise", "Colon", "Gill", "Alvarado", "Greer", "Padilla", "Simon", "Waters", "Nunez", "Ballard", "Schwartz", "Mcbride", "Houston", "Christensen", "Klein", "Pratt", "Briggs", "Parsons", "Mclaughlin", "Zimmerman", "French", "Buchanan", "Moran", "Copeland", "Roy", "Pittman", "Brady", "Mccormick", "Holloway", "Brock", "Poole", "Frank", "Logan", "Owen", "Bass", "Marsh", "Drake", "Wong", "Jefferson", "Park", "Morton", "Abbott", "Sparks", "Patrick", "Norton", "Huff", "Clayton", "Massey", "Lloyd", "Figueroa", "Carson", "Bowers", "Roberson", "Barton", "Tran", "Lamb", "Harrington", "Casey", "Boone", "Cortez", "Clarke", "Mathis", "Singleton", "Wilkins", "Cain", "Bryan", "Underwood", "Hogan", "Mckenzie", "Collier", "Luna", "Phelps", "Mcguire", "Allison", "Bridges", "Wilkerson", "Nash", "Summers", "Atkins", "Wilcox", "Pitts", "Conley", "Marquez", "Burnett", "Richard", "Cochran", "Chase", "Davenport", "Hood", "Gates", "Clay", "Ayala", "Sawyer", "Roman", "Vazquez", "Dickerson", "Hodge", "Acosta", "Flynn", "Espinoza", "Nicholson", "Monroe", "Wolf", "Morrow", "Kirk", "Randall", "Anthony", "Whitaker", "Oconnor", "Skinner", "Ware", "Molina", "Kirby", "Huffman", "Bradford", "Charles", "Gilmore", "Dominguez", "Oneal", "Bruce", "Lang", "Combs", "Kramer", "Heath", "Hancock", "Gallagher", "Gaines", "Shaffer", "Short", "Wiggins", "Mathews", "Mcclain", "Fischer", "Wall", "Small", "Melton", "Hensley", "Bond", "Dyer", "Cameron", "Grimes", "Contreras", "Christian", "Wyatt", "Baxter", "Snow", "Mosley", "Shepherd", "Larsen", "Hoover", "Beasley", "Glenn", "Petersen", "Whitehead", "Meyers", "Keith", "Garrison", "Vincent", "Shields", "Horn", "Savage", "Olsen", "Schroeder", "Hartman", "Woodard", "Mueller", "Kemp", "Deleon", "Booth", "Patel", "Calhoun", "Wiley", "Eaton", "Cline", "Navarro", "Harrell", "Lester", "Humphrey", "Parrish", "Duran", "Hutchinson", "Hess", "Dorsey", "Bullock", "Robles", "Beard", "Dalton", "Avila", "Vance", "Rich", "Blackwell", "York", "Johns", "Blankenship", "Trevino", "Salinas", "Campos", "Pruitt", "Moses", "Callahan", "Golden", "Montoya", "Hardin", "Guerra", "Mcdowell", "Carey", "Stafford", "Gallegos", "Henson", "Wilkinson", "Booker", "Merritt", "Miranda", "Atkinson", "Orr", "Decker", "Hobbs", "Preston", "Tanner", "Knox", "Pacheco", "Stephenson", "Glass", "Rojas", "Serrano", "Marks", "Hickman", "English", "Sweeney", "Strong", "Prince", "Mcclure", "Conway", "Walter", "Roth", "Maynard", "Farrell", "Lowery", "Hurst", "Nixon", "Weiss", "Trujillo", "Ellison", "Sloan", "Juarez", "Winters", "Mclean", "Randolph", "Leon", "Boyer", "Villarreal", "Mccall", "Gentry", "Carrillo", "Kent", "Ayers", "Lara", "Shannon", "Sexton", "Pace", "Hull", "Leblanc", "Browning", "Velasquez", "Leach", "Chang", "House", "Sellers", "Herring", "Noble", "Foley", "Bartlett", "Mercado", "Landry", "Durham", "Walls", "Barr", "Mckee", "Bauer", "Rivers", "Everett", "Bradshaw", "Pugh", "Velez", "Rush", "Estes", "Dodson", "Morse", "Sheppard", "Weeks", "Camacho", "Bean", "Barron", "Livingston", "Middleton", "Spears", "Branch", "Blevins", "Chen", "Kerr", "Mcconnell", "Hatfield", "Harding", "Ashley", "Solis", "Herman", "Frost", "Giles", "Blackburn", "William", "Pennington", "Woodward", "Finley", "Mcintosh", "Koch", "Best", "Solomon", "Mccullough", "Dudley", "Nolan", "Blanchard", "Rivas", "Brennan", "Mejia", "Kane", "Benton", "Joyce", "Buckley", "Haley", "Valentine", "Maddox", "Russo", "Mcknight", "Buck", "Moon", "Mcmillan", "Crosby", "Berg", "Dotson", "Mays", "Roach", "Church", "Chan", "Richmond", "Meadows", "Faulkner", "Oneill", "Knapp", "Kline", "Barry", "Ochoa", "Jacobson", "Gay", "Avery", "Hendricks", "Horne", "Shepard", "Hebert", "Cherry", "Cardenas", "Mcintyre", "Whitney", "Waller", "Holman", "Donaldson", "Cantu", "Terrell", "Morin", "Gillespie", "Fuentes", "Tillman", "Sanford", "Bentley", "Peck", "Key", "Salas", "Rollins", "Gamble", "Dickson", "Battle", "Santana", "Cabrera", "Cervantes", "Howe", "Hinton", "Hurley", "Spence", "Zamora", "Yang", "Mcneil", "Suarez", "Case", "Petty", "Gould", "Mcfarland", "Sampson", "Carver", "Bray", "Rosario", "Macdonald", "Stout", "Hester", "Melendez", "Dillon", "Farley", "Hopper", "Galloway", "Potts", "Bernard", "Joyner", "Stein", "Aguirre", "Osborn", "Mercer", "Bender", "Franco", "Rowland", "Sykes", "Benjamin", "Travis", "Pickett", "Crane", "Sears", "Mayo", "Dunlap", "Hayden", "Wilder", "Mckay", "Coffey", "Mccarty", "Ewing", "Cooley", "Vaughan", "Bonner", "Cotton", "Holder", "Stark", "Ferrell", "Cantrell", "Fulton", "Lynn", "Lott", "Calderon", "Rosa", "Pollard", "Hooper", "Burch", "Mullen", "Fry", "Riddle", "Levy", "David", "Duke", "Odonnell", "Guy", "Michael", "Britt", "Frederick", "Daugherty", "Berger", "Dillard", "Alston", "Jarvis", "Frye", "Riggs", "Chaney", "Odom", "Duffy", "Fitzpatrick", "Valenzuela", "Merrill", "Mayer", "Alford", "Mcpherson", "Acevedo", "Donovan", "Barrera", "Albert", "Cote", "Reilly", "Compton", "Raymond", "Mooney", "Mcgowan", "Craft", "Cleveland", "Clemons", "Wynn", "Nielsen", "Baird", "Stanton", "Snider", "Rosales", "Bright", "Witt", "Stuart", "Hays", "Holden", "Rutledge", "Kinney", "Clements", "Castaneda", "Slater", "Hahn", "Emerson", "Conrad", "Burks", "Delaney", "Pate", "Lancaster", "Sweet", "Justice", "Tyson", "Sharpe", "Whitfield", "Talley", "Macias", "Irwin", "Burris", "Ratliff", "Mccray", "Madden", "Kaufman", "Beach", "Goff", "Cash", "Bolton", "Mcfadden", "Levine", "Good", "Byers", "Kirkland", "Kidd", "Workman", "Carney", "Dale", "Mcleod", "Holcomb", "England", "Finch", "Head", "Burt", "Hendrix", "Sosa", "Haney", "Franks", "Sargent", "Nieves", "Downs", "Rasmussen", "Bird", "Hewitt", "Lindsay", "Le", "Foreman", "Valencia", "Oneil", "Delacruz", "Vinson", "Dejesus", "Hyde", "Forbes", "Gilliam", "Guthrie", "Wooten", "Huber", "Barlow", "Boyle", "Mcmahon", "Buckner", "Rocha", "Puckett", "Langley", "Knowles", "Cooke", "Velazquez", "Whitley", "Noel", "Vang", "Shea", "Rouse", "Hartley", "Mayfield", "Elder", "Rankin", "Hanna", "Cowan", "Lucero", "Arroyo", "Slaughter", "Haas", "Oconnell", "Minor", "Kendrick", "Shirley", "Kendall", "Boucher", "Archer", "Boggs", "Odell", "Dougherty", "Andersen", "Newell", "Crowe", "Wang", "Friedman", "Bland", "Swain", "Holley", "Felix", "Pearce", "Childs", "Yarbrough", "Galvan", "Proctor", "Meeks", "Lozano", "Mora", "Rangel", "Bacon", "Villanueva", "Schaefer", "Rosado", "Helms", "Boyce", "Goss", "Stinson", "Smart", "Lake", "Ibarra", "Hutchins", "Covington", "Reyna", "Gregg", "Werner", "Crowley", "Hatcher", "Mackey", "Bunch", "Womack", "Polk", "Jamison", "Dodd", "Childress", "Childers", "Camp", "Villa", "Dye", "Springer", "Mahoney", "Dailey", "Belcher", "Lockhart", "Griggs", "Costa", "Connor", "Brandt", "Winter", "Walden", "Moser", "Tracy", "Tatum", "Mccann", "Akers", "Lutz", "Pryor", "Law", "Orozco", "Mcallister", "Lugo", "Davies", "Shoemaker", "Madison", "Rutherford", "Newsome", "Magee", "Chamberlain", "Blanton", "Simms", "Godfrey", "Flanagan", "Crum", "Cordova", "Escobar", "Downing", "Sinclair", "Donahue", "Krueger", "Mcginnis", "Gore", "Farris", "Webber", "Corbett", "Andrade", "Starr", "Lyon", "Yoder", "Hastings", "Mcgrath", "Spivey", "Krause", "Harden", "Crabtree", "Kirkpatrick", "Hollis", "Brandon", "Arrington", "Ervin", "Clifton", "Ritter", "Mcghee", "Bolden", "Maloney", "Gagnon", "Dunbar", "Ponce", "Pike", "Mayes", "Heard", "Beatty", "Mobley", "Kimball", "Butts", "Montes", "Herbert", "Grady", "Eldridge", "Braun", "Hamm", "Gibbons", "Seymour", "Moyer", "Manley", "Herron", "Plummer", "Elmore", "Cramer", "Gary", "Rucker", "Hilton", "Blue", "Pierson", "Fontenot", "Field", "Ganaye" };
            static string[] Occupations = new string[] { "Actor", "Actuary", "Advertising", "Advocate", "Aeronautical Engineer", "Aerospace Industry Trades", "Agricultural Economist", "Agricultural Engineer", "Agricultural Extension Officer", "Agricultural Inspector", "Agricultural Technician", "Agriculture", "Agriculturist", "Agronomist", "Air Traffic Controller", "Ambulance Emergency Care Worker", "Animal Scientist", "Anthropologist", "Aquatic Scientist", "Archaeologist", "Architect", "Architectural Technologist", "Archivist", "Area Manager", "Armament Fitter", "Armature Winder", "Art Editor", "Artist", "Assayer Sampler", "Assembly Line Worker", "Assistant Draughtsman", "Astronomer", "Attorney", "Auctioneer", "Auditor", "Automotive Body Repairer", "Automotive Electrician", "Automotive Mechinist", "Automotive Trimmer", "Babysitting Career", "Banking Career", "Beer Brewing", "Biochemist", "Biokineticist", "Biologist", "Biomedical Engineer", "Biomedicaltechnologist", "Blacksmith", "Boilermaker", "Bookbinder", "Bookkeeper", "Botanist", "Branch Manager", "Bricklayer", "Bus Driver", "Business Analyst", "Business Economist", "Butler", "Cabin Attendant", "Carpenter", "Cartographer", "Cashier", "Ceramics Technologist", "Chartered Accountant", "Chartered Management Accountant", "Chartered Secretary", "Chemical Engineer", "Chemist", "Chiropractor", "City Treasurer", "Civil Engineer", "Civil Investigator", "Cleaner", "Clergyman", "Clerk", "Clinical Engineering", "Clinical Technologist", "Clothing Designer", "Clothing Manager", "Coal Technologist", "Cobbler", "Committee Clerk", "Computer Industry", "Concrete Technician", "Conservation And Wildlife", "Construction Manager", "Copy Writer", "Correctional Services", "Costume Designer", "Crane Operator", "Credit Controller", "Crop Protection And Animal Health", "Customer And Excise Officer", "Customer Service Agent", "Dancer", "Data Capturer", "Database Administrator", "Dealer In Oriental Carpets", "Decor Designer", "Dental Assistant And Oral Hygienist", "Dental Technician", "Dental Therapist", "Dentist", "Detective", "Diamond Cutting", "Diesel Fitter", "Diesel Loco Driver", "Diesel Mechanic", "Die-Sinker And Engraver", "Dietician", "Diver", "Dj", "Domestic Appliance Mechanician", "Domestic Personnel", "Domestic Radio And Television Mechanician", "Domestic Worker", "Draughtsman", "Driver And Stacker", "Earth Moving Equipment Mechanic", "Ecologist", "Economist Technician", "Editor", "Eeg Technician", "Electrical And Electronic Engineer", "Electrical Engineering Technician", "Electrician", "Electrician (Construction)", "Engineering", "Engineering Technician", "Entomologist", "Environmental Health Officer", "Estate Agent", "Explosive Expert", "Explosive Technologist", "Extractive Metallurgist", "Farm Foreman", "Farm Worker", "Farmer", "Fashion Buyer", "Film And Production", "Financial And Investment Manager", "Fire-Fighter", "Fireman At The Airport", "Fitter And Turner", "Flight Engineer", "Florist", "Food Scientist And Technologist", "Footwear", "Forester Service", "Funeral Director", "Furrier", "Game Ranger", "Gardener", "Geneticist", "Geographer", "Geologist", "Geotechnologist", "Goldsmith And Jeweller", "Grain Grader", "Graphic Designer", "Gravure Machine Minder", "Hairdresser", "Herpetologist", "Home Economist", "Homoeopath", "Horticulturist", "Hospital Porter", "Hospitality Industry", "Human Resource Manager", "Hydrologist", "Ichthyologist", "Industrial Designer", "Industrial Engineer", "Industrial Engineering Technologist", "Industrial Technician", "Inspector", "Instrument Maker", "Insurance", "Interior Designer", "Interpreter", "Inventory And Store Manager", "Jeweler", "Jockey", "Joiner And Woodmachinist", "Journalist", "Knitter", "Labourer", "Land Surveyor", "Landscape Architect", "Law", "Learner Official", "Leather Chemist", "Leather Worker", "Lecturer", "Librarian", "Life-Guard", "Lift Mechanic", "Light Delivery Van Driver", "Linesman", "Locksmith", "Machine Operator", "Machine Worker", "Magistrate", "Mail Handler", "Make-Up Artist", "Management Consultant", "Manager", "Marine Biologist", "Marketing", "Marketing Manager", "Materials Engineer", "Mathematician", "Matron", "Meat Cutting Technician", "Mechanical Engineer", "Medical Doctor", "Medical Orthotist Prosthetist", "Medical Physicist", "Merchandise Planner", "Messenger", "Meteorological Technician", "Meteorologist", "Meter-Reader", "Microbiologist", "Mine Surveyor", "Miner", "Mining Engineer", "Model", "Model Builder", "Motor Mechanic", "Musician", "Nature Conservator", "Navigating Officer", "Navigator", "Nuclear Scientist", "Nursing", "Nutritionist", "Occupational Therapist", "Oceanographer", "Operations Researcher", "Optical Dispenser", "Optical Technician", "Optometrist", "Ornithologist", "Paint Technician", "Painter And Decorator", "Paper Technologist", "Patent Attorney", "Personal Trainer", "Personnel Consultant", "Petroleum Technologist", "Pharmacist", "Pharmacist Assistant", "Photographer", "Physicist", "Physiologist", "Physiotherapist", "Piano Tuner", "Pilot", "Plumber", "Podiatrist", "Police Officer", "Post Office Clerk", "Power Plant Operator", "Private Secretary", "Production Manager", "Project Manager", "Projectionist", "Psychologist", "Psychometrist", "Public Relations Practitioner", "Purchasing Manager", "Quality Control Inspector", "Quantity Surveyor", "Radiation Protectionist", "Radio", "Radiographer", "Receptionist", "Recreation Manager", "Rigger", "Road Construction Plant Operator", "Roofer", "Rubber Technologist", "Sales Representative", "Salesperson", "Saw Operator", "Scale Fitter", "Sea Transport Worker", "Secretary", "Security Officer", "Sheetmetal Worker", "Shop Assistant", "Shopfitter", "Singer", "Social Worker", "Sociologist", "Soil Scientist", "Speech And Language Therapist", "Sport Manager", "Spray Painter", "Statistician", "Swimming Pool Superintendent", "Systems Analyst", "Tailor", "Taxidermist", "Teacher", "Technical Illustrator", "Technical Writer", "Teller", "Terminologist", "Textile Designer", "Theatre Technology", "Tourism Manager", "Traffic Officer", "Translator", "Travel Agent", "Typist", "Valuer And Appraiser", "Vehicle Driver", "Veterinary Nurse", "Veterinary Surgeon", "Viticulturist", "Watchmaker", "Weather Observer", "Weaver", "Welder", "Wood Scientist", "Wood Technologist", "Yard Official", "Zoologist" };
            private static string[] Cities = new string[] { "New York", "Los Angeles", "Chicago", "Houston", "Philadelphia", "Phoenix", "San Antonio", "San Diego", "Dallas", "San Jose", "Jacksonville", "Indianapolis", "San Francisco", "Austin", "Columbus", "Fort Worth", "Charlotte", "Detroit", "El Paso", "Memphis", "Baltimore", "Boston", "Seattle", "Washington", "Nashville", "Denver", "Louisville", "Milwaukee", "Portland", "Las Vegas", "Albuquerque", "Tucson", "Fresno", "Sacramento", "Long Beach", "Kansas City", "Atlanta", "Omaha", "Raleigh", "Miami", "Cleveland", "Tulsa", "Oakland", "Minneapolis", "Wichita", "Bakersfield", "New Orleans", "Honolulu", "Tampa", "Aurora", "Santa Ana", "Pittsburgh", "Riverside", "Cincinnati", "Lexington", "Anchorage", "Stockton", "Toledo", "Saint Paul", "Newark", "Greensboro", "Buffalo", "Lincoln", "Fort Wayne", "Norfolk", "Orlando", "Laredo", "Madison", "Winston-Salem", "Lubbock", "Baton Rouge", "Reno", "Chesapeake", "Scottsdale", "Birmingham", "Rochester", "Spokane", "Montgomery", "Boise", "Richmond", "Des Moines", "Modesto", "Fayetteville", "Shreveport", "Akron", "Tacoma", "Oxnard", "Augusta", "Mobile", "Little Rock", "Amarillo", "Grand Rapids", "Tallahassee", "Worcester", "Newport News", "Huntsville", "Knoxville", "Providence", "Brownsville", "Jackson", "Santa Rosa", "Chattanooga", "Ontario", "Springfield", "Lancaster", "Eugene", "Salem", "Peoria", "Sioux Falls", "Rockford", "Palmdale", "Corona", "Salinas", "Torrance", "Syracuse", "Bridgeport", "Hayward", "Dayton", "Alexandria", "Savannah", "Fullerton", "Clarksville", "McAllen", "New Haven", "Columbia", "Killeen", "Topeka", "Cedar Rapids", "Olathe", "Elizabeth", "Waco", "Hartford", "Visalia", "Gainesville", "Concord", "Miramar", "Lafayette", "Charleston", "Beaumont", "Allentown", "Evansville", "Abilene", "Athens", "Lansing", "Ann Arbor", "El Monte", "Provo", "Midland", "Norman", "Manchester", "Pueblo", "Wilmington", "Fargo", "Carlsbad", "Fairfield", "Billings", "Green Bay", "Burbank", "Flint", "Erie", "South Bend" };

            public static Client RandomClient()
            {

                var client = new Client();
                if (rnd.Next(2) == 0)
                {
                    client.FirstName = RandomEntry(MaleFirstNames);
                    client.MiddleName = (rnd.Next(100) >= 90 ? null : RandomEntry(MaleFirstNames));
                }
                else
                {
                    client.FirstName = RandomEntry(FemaleFirstNames);
                    client.MiddleName = (rnd.Next(100) >= 90 ? null : RandomEntry(FemaleFirstNames));
                }
                client.LastName = RandomEntry(LastNames);
                client.City = RandomEntry(Cities);

                client.Occupation = (rnd.Next(100) >= 95 ? null : RandomEntry(Occupations));
                return client;
            }

        }
    }
}
