using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace DroneDeliverySystem
{
    public class Drone
    {
        public int Id { get; set; }
        public string Model { get; set; }
        public double MaxPayload { get; set; }
        public string Status { get; set; } = "Idle";
        public (double Lat, double Lon) Location { get; set; } = (0, 0);
    }

    public class Delivery
    {
        public int Id { get; set; }
        public string Package { get; set; }
        public double Weight { get; set; }
        public (double Lat, double Lon) Destination { get; set; }
        public string Status { get; set; } = "Pending";
        public int DroneId { get; set; } = -1;
    }

    class Program
    {
        static List<Drone> drones = new List<Drone>();
        static List<Delivery> deliveries = new List<Delivery>();
        static int droneCounter = 0;
        static int deliveryCounter = 0;

        static void Main(string[] args)
        {
            string jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "drones.json");
            string xmlFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "drones.xml");
            string transformedXmlFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "transformed_drones.xml");
            string transformedJsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "transformed_drones.json");

            if (File.Exists(jsonFilePath))
            {
                var jsonData = File.ReadAllText(jsonFilePath);
                drones = JsonConvert.DeserializeObject<List<Drone>>(jsonData);
            }

            if (File.Exists(xmlFilePath))
            {
                XDocument xdoc = XDocument.Load(xmlFilePath);
                drones = xdoc.Descendants("Drone")
                             .Select(x => new Drone
                             {
                                 Id = int.Parse(x.Element("Id").Value),
                                 Model = x.Element("Model").Value,
                                 MaxPayload = double.Parse(x.Element("MaxPayload").Value),
                                 Status = x.Element("Status").Value,
                                 Location = (double.Parse(x.Element("Lat").Value), double.Parse(x.Element("Lon").Value))
                             }).ToList();
            }

            while (true)
            {
                Console.WriteLine("\nChoisissez une option:");
                Console.WriteLine("1. Ajouter un drone (Modèle et charge utile maximale)");
                Console.WriteLine("2. Supprimer un drone (ID du drone)");
                Console.WriteLine("3. Lister les drones (Affiche tous les drones)");
                Console.WriteLine("4. Planifier une livraison (Nom du colis, poids, destination)");
                Console.WriteLine("5. Suivre les livraisons (Affiche toutes les livraisons)");
                Console.WriteLine("6. Transformer les données JSON en XML");
                Console.WriteLine("7. Transformer les données XML en JSON");
                Console.WriteLine("8. Rechercher par nom de drone (Entrez le modèle du drone)");
                Console.WriteLine("9. Trier les drones par charge utile");
                Console.WriteLine("0. Quitter et sauvegarder les données");

                var choice = Console.ReadLine();
                switch (choice)
                {
                    case "1":
                        AddDrone();
                        break;
                    case "2":
                        RemoveDrone();
                        break;
                    case "3":
                        ListDrones();
                        break;
                    case "4":
                        PlanDelivery();
                        break;
                    case "5":
                        TrackDeliveries();
                        break;
                    case "6":
                        TransformJsonToXml();
                        break;
                    case "7":
                        TransformXmlToJson();
                        break;
                    case "8":
                        SearchDroneByName();
                        break;
                    case "9":
                        SortDronesByPayload();
                        break;
                    case "0":
                        SaveData(jsonFilePath, xmlFilePath);
                        return;
                    default:
                        Console.WriteLine("Choix invalide, veuillez réessayer.");
                        break;
                }
            }
        }

        static void AddDrone()
        {
            Console.WriteLine("Entrez le modèle du drone (exemple: DJI Phantom 4) :");
            string model = Console.ReadLine();
            Console.WriteLine("Entrez la charge utile maximale du drone en kilogrammes (exemple: 1.5) :");
            double maxPayload = double.Parse(Console.ReadLine());
            drones.Add(new Drone
            {
                Id = ++droneCounter,
                Model = model,
                MaxPayload = maxPayload
            });
            Console.WriteLine("Drone ajouté avec succès !");
        }

        static void RemoveDrone()
        {
            Console.WriteLine("Entrez l'ID du drone à supprimer :");
            int id = int.Parse(Console.ReadLine());
            var drone = drones.FirstOrDefault(d => d.Id == id);
            if (drone != null)
            {
                drones.Remove(drone);
                Console.WriteLine("Drone supprimé avec succès !");
            }
            else
            {
                Console.WriteLine("Drone introuvable.");
            }
        }

        static void ListDrones()
        {
            Console.WriteLine("\nListe des drones :");
            foreach (var drone in drones)
            {
                Console.WriteLine($"ID: {drone.Id}, Modèle: {drone.Model}, Charge Utile Max: {drone.MaxPayload} kg, Statut: {drone.Status}, Localisation: ({drone.Location.Lat}, {drone.Location.Lon})");
            }
        }

        static void PlanDelivery()
        {
            Console.WriteLine("Entrez le nom du colis (exemple: Médicaments) :");
            string package = Console.ReadLine();
            Console.WriteLine("Entrez le poids du colis en kilogrammes (exemple: 2.0) :");
            double weight = double.Parse(Console.ReadLine());
            Console.WriteLine("Entrez la latitude de la destination (exemple: 48.8566) :");
            double lat = double.Parse(Console.ReadLine());
            Console.WriteLine("Entrez la longitude de la destination (exemple: 2.3522) :");
            double lon = double.Parse(Console.ReadLine());

            var availableDrone = drones.FirstOrDefault(d => d.Status == "Idle" && d.MaxPayload >= weight);
            if (availableDrone != null)
            {
                var delivery = new Delivery
                {
                    Id = ++deliveryCounter,
                    Package = package,
                    Weight = weight,
                    Destination = (lat, lon),
                    Status = "In Progress",
                    DroneId = availableDrone.Id
                };
                deliveries.Add(delivery);
                availableDrone.Status = "In Flight";
                Console.WriteLine("Livraison planifiée avec succès !");
            }
            else
            {
                Console.WriteLine("Aucun drone disponible pour cette livraison.");
            }
        }

        static void TrackDeliveries()
        {
            Console.WriteLine("\nSuivi des livraisons :");
            foreach (var delivery in deliveries)
            {
                var drone = drones.FirstOrDefault(d => d.Id == delivery.DroneId);
                Console.WriteLine($"ID: {delivery.Id}, Colis: {delivery.Package}, Poids: {delivery.Weight} kg, Destination: ({delivery.Destination.Lat}, {delivery.Destination.Lon}), Statut: {delivery.Status}, Drone: {drone?.Model ?? "N/A"}");
            }
        }

        static void TransformJsonToXml()
        {
            var transformedXml = new XElement("Drones",
                drones.Select(d => new XElement("Drone",
                    new XElement("Id", d.Id),
                    new XElement("Model", d.Model),
                    new XElement("MaxPayload", d.MaxPayload),
                    new XElement("Status", d.Status),
                    new XElement("Lat", d.Location.Lat),
                    new XElement("Lon", d.Location.Lon)
                ))
            );
            transformedXml.Save("transformed_drones.xml");
            Console.WriteLine("Les données JSON ont été transformées en XML et sauvegardées dans transformed_drones.xml");
        }

        static void TransformXmlToJson()
        {
            var json = JsonConvert.SerializeObject(drones, Formatting.Indented);
            File.WriteAllText("transformed_drones.json", json);
            Console.WriteLine("Les données XML ont été transformées en JSON et sauvegardées dans transformed_drones.json");
        }

        static void SearchDroneByName()
        {
            Console.WriteLine("Entrez le modèle du drone à rechercher (exemple: DJI Phantom 4) :");
            string model = Console.ReadLine();
            var foundDrones = drones.Where(d => d.Model.Equals(model, StringComparison.OrdinalIgnoreCase)).ToList();

            Console.WriteLine($"\nRésultats de la recherche pour {model} :");
            foreach (var drone in foundDrones)
            {
                Console.WriteLine($"ID: {drone.Id}, Modèle: {drone.Model}, Charge Utile Max: {drone.MaxPayload} kg, Statut: {drone.Status}, Localisation: ({drone.Location.Lat}, {drone.Location.Lon})");
            }
        }

        static void SortDronesByPayload()
        {
            var sortedDrones = drones.OrderBy(d => d.MaxPayload).ToList();
            Console.WriteLine("\nDrones triés par charge utile maximale :");
            foreach (var drone in sortedDrones)
            {
                Console.WriteLine($"ID: {drone.Id}, Modèle: {drone.Model}, Charge Utile Max: {drone.MaxPayload} kg, Statut: {drone.Status}, Localisation: ({drone.Location.Lat}, {drone.Location.Lon})");
            }
        }

        static void SaveData(string jsonFilePath, string xmlFilePath)
        {
            var json = JsonConvert.SerializeObject(drones, Formatting.Indented);
            File.WriteAllText(jsonFilePath, json);

            var transformedXml = new XElement("Drones",
                drones.Select(d => new XElement("Drone",
                    new XElement("Id", d.Id),
                    new XElement("Model", d.Model),
                    new XElement("MaxPayload", d.MaxPayload),
                    new XElement("Status", d.Status),
                    new XElement("Lat", d.Location.Lat),
                    new XElement("Lon", d.Location.Lon)
                ))
            );
            transformedXml.Save(xmlFilePath);
        }
    }
}
