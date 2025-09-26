using System.Text.Json;

class Program
{
    public class Inventory  //for inventory
    {
        public required Dictionary<string, Item> items { get; set; }
        public required int total_weight { get; set; }
    }

    public class Item //for items
    {
        public int id { get; set; }
        public required string name { get; set; }
        public int weight { get; set; }
        public int quantity { get; set; }
    }

    public class Root //root class for json (check storage.json for better understand)
    {
        public required Inventory inventory { get; set; }
        public required List<Item> location { get; set; }
    }

    static void Main()
    {
        Console.WriteLine("Interim Challenge Task\n");
        Console.WriteLine("Write help to get a list of all commands\n"); //intro messages

        while (true)
        {
            string jsonData = File.ReadAllText("storage.json");
            Root data = JsonSerializer.Deserialize<Root>(jsonData); //deserialize json to object

            Console.Write("> ");
            string input = Console.ReadLine(); //read user input

            if (string.IsNullOrWhiteSpace(input))
                continue;

            string[] parts = input.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries); //okay, so we have comamand and name of item, we jus split it in 2 variables
            string command = parts[0].ToLower(); 
            string args = parts.Length > 1 ? parts[1] : "";

            switch (command)
            {
                case "help":
                    Console.WriteLine("List of all commands:\n");
                    Console.WriteLine("Pick <item name> - pick up item to your inventory\n");
                    Console.WriteLine("Drop <item name> - drop item from your inventory\n");
                    Console.WriteLine("Inventory - check your inventory\n");
                    Console.WriteLine("Weight - check how much does your inventory weight\n");
                    break;

                case "pick":
                    Item foundItem = data.location.Find(item => item.name.Equals(args, StringComparison.OrdinalIgnoreCase)); //find item in location
                    if (foundItem == null) //if item not found
                    {
                        Console.WriteLine("Item is does not exist. Please try again");
                        break;
                    }
                    else if (foundItem.quantity <= 0) //if item out of stock
                    {
                        Console.WriteLine("Item is out of stock.");
                        break;
                    }
                    else if (data.inventory.total_weight + foundItem.weight > 10) //check if weight limit exceeded
                    {
                        Console.WriteLine("You can't pick up this item, it will exceed your inventory weight limit.");
                        break;
                    }
                    else
                    {
                        if (data.inventory.items.ContainsKey(foundItem.name)) //if item already in inventory, increase quantity. if not, add new item
                        {
                            data.inventory.items[foundItem.name].quantity++;
                        }
                        else
                        {
                            data.inventory.items[foundItem.name] = new Item
                            {
                                id = foundItem.id,
                                name = foundItem.name,
                                weight = foundItem.weight,
                                quantity = 1
                            };
                        }
                        data.inventory.total_weight += foundItem.weight; //update total weight
                        foundItem.quantity -= 1; //decrease quantity in location
                        string updatedPickJson = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                        File.WriteAllText("storage.json", updatedPickJson); //write updated data back to json file
                        Console.WriteLine($"You picked up: {foundItem.name}"); //answer to user
                    }
                    break;
                case "drop":
                    var key = data.inventory.items.Keys.FirstOrDefault(k => k.Equals(args, StringComparison.OrdinalIgnoreCase)); //find item in inventory (because user can write name with lowercase, and the name is uppercase we changing case)
                    if (key == null)  //if item not found or user dont have this item
                    {
                        Console.WriteLine("Item is does not exist or you dont have this item. Please try again");
                        break;
                    }
                    Item dropItem = data.inventory.items[key]; //get item from inventory (bc key is just a name)
                    Item dropLocationItem = data.location.Find(item => item.name.Equals(dropItem.name, StringComparison.OrdinalIgnoreCase));  //u will get it later

                    if (dropItem.quantity > 1)  //if user have more than 1 item, decrease quantity. if not, remove item from inventory
                    {
                        data.inventory.items[key].quantity--;
                    }
                    else
                    {
                        data.inventory.items.Remove(key);
                    }
                    data.inventory.total_weight -= dropItem.weight; //update total weight
                    dropLocationItem.quantity += 1; //so dropLocationItem basically was needed just to increase quantity in location
                    string updatedDropJson = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText("storage.json", updatedDropJson);  //again write updated data back to json file
                    Console.WriteLine($"You drop out: {dropItem.name}"); //again answer to user
                    break;
                case "inventory":  //check inventory (this might be easy)
                    if (data.inventory.items.Count == 0)
                    {
                        Console.WriteLine("Your inventar is empty.");
                        break;
                    }
                    foreach (var item in data.inventory.items)
                    {
                        Item invItem = item.Value;
                        Console.WriteLine($"{invItem.name} (x{invItem.quantity}) - Weight: {invItem.weight}" + (invItem.quantity > 1 ? $" (Total Weight: {invItem.quantity * invItem.weight})" : ""));
                    }
                    break;
                case "weight":
                    Console.WriteLine($"Total inventory weight: {data.inventory.total_weight}/10");
                    break;
                default:
                    Console.WriteLine("Unknown command");
                    break;
            }
        }
    }
}
