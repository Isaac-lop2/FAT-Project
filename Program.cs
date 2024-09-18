using System.Text.Json;

class Program
{
    static string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    static string folderPath = Path.Combine(desktopPath, "SistemaFAT");
    static string fatTablePath = Path.Combine(folderPath, "fatTable.json");

    static List<FatTableEntry> fatTable = new List<FatTableEntry>();

    static void Main()
    {
    
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        
        if (File.Exists(fatTablePath))
        {
            string jsonFromFile = File.ReadAllText(fatTablePath);
            fatTable = JsonSerializer.Deserialize<List<FatTableEntry>>(jsonFromFile) ?? new List<FatTableEntry>();
        }

        while (true)
        {
            Console.WriteLine("\nSeleccione una opción:");
            Console.WriteLine("1. Crear un archivo y agregar datos");
            Console.WriteLine("2. Listar archivos");
            Console.WriteLine("3. Abrir un archivo");
            Console.WriteLine("4. Modificar un archivo");
            Console.WriteLine("5. Eliminar un archivo");
            Console.WriteLine("6. Recuperar un archivo");
            Console.WriteLine("7. Salir");

            string option = Console.ReadLine();

            switch (option)
            {
                case "1":
                    CrearArchivo();
                    break;
                case "2":
                    ListarArchivos(false);
                    break;
                case "3":
                    AbrirArchivo();
                    break;
                case "4":
                    ModificarArchivo();
                    break;
                case "5":
                    EliminarArchivo();
                    break;
                case "6":
                    RecuperarArchivo();
                    break;
                case "7":
                    return;
                default:
                    Console.WriteLine("Opción no válida. Por favor, intente de nuevo.");
                    break;
            }
        }
    }

    static void CrearArchivo()
    {
        Console.Write("Ingrese el nombre del archivo: ");
        string nombreArchivo = Console.ReadLine();

        Console.Write("Ingrese los datos del archivo: ");
        string contenido = Console.ReadLine();

        
        List<string> segmentos = SegmentarDatos(contenido, 20);

        
        string primerSegmentoPath = GuardarSegmentos(segmentos);

        
        FatTableEntry newEntry = new FatTableEntry
        {
            Nombre = nombreArchivo,
            RutaInicial = primerSegmentoPath,
            EnPapelera = false,
            TotalCaracteres = contenido.Length,
            FechaCreacion = DateTime.Now,
            FechaModificacion = DateTime.Now
        };

        fatTable.Add(newEntry);
        GuardarFatTable();
        Console.WriteLine("Archivo creado exitosamente.");
    }

    static void ListarArchivos(bool listarEliminados)
    {
        var archivos = fatTable.Where(f => f.EnPapelera == listarEliminados).ToList();

        if (archivos.Count == 0)
        {
            Console.WriteLine(listarEliminados ? "No hay archivos eliminados." : "No hay archivos.");
            return;
        }

        Console.WriteLine("\nListado de archivos:");
        for (int i = 0; i < archivos.Count; i++)
        {
            var archivo = archivos[i];
            Console.WriteLine($"{i + 1}. Nombre: {archivo.Nombre}, Tamaño: {archivo.TotalCaracteres} caracteres, " +
                $"Creación: {archivo.FechaCreacion}, Modificación: {archivo.FechaModificacion}");
        }
    }

    static void AbrirArchivo()
    {
        ListarArchivos(false);
        Console.Write("\nSeleccione el número del archivo que desea abrir: ");
        if (int.TryParse(Console.ReadLine(), out int index) && index > 0 && index <= fatTable.Count)
        {
            var archivo = fatTable[index - 1];
            string contenido = LeerArchivoCompleto(archivo.RutaInicial);

            Console.WriteLine($"\nNombre: {archivo.Nombre}");
            Console.WriteLine($"Tamaño: {archivo.TotalCaracteres} caracteres");
            Console.WriteLine($"Creación: {archivo.FechaCreacion}, Modificación: {archivo.FechaModificacion}");
            Console.WriteLine($"Contenido:\n{contenido}");
        }
        else
        {
            Console.WriteLine("Selección no válida.");
        }
    }

    static void ModificarArchivo()
    {
        ListarArchivos(false);
        Console.Write("\nSeleccione el número del archivo que desea modificar: ");
        if (int.TryParse(Console.ReadLine(), out int index) && index > 0 && index <= fatTable.Count)
        {
            var archivo = fatTable[index - 1];
            string contenidoActual = LeerArchivoCompleto(archivo.RutaInicial);

            Console.WriteLine($"Contenido actual:\n{contenidoActual}");
            Console.WriteLine("\nIngrese el nuevo contenido (presione ESC para salir): ");
            string nuevoContenido = LeerEntrada();

            if (!string.IsNullOrEmpty(nuevoContenido))
            {
                Console.Write("¿Desea guardar los cambios? (S/N): ");
                if (Console.ReadLine().Trim().ToUpper() == "S")
                {
                   
                    EliminarArchivosSegmentados(archivo.RutaInicial);

                    
                    List<string> segmentos = SegmentarDatos(nuevoContenido, 20);
                    archivo.RutaInicial = GuardarSegmentos(segmentos);
                    archivo.TotalCaracteres = nuevoContenido.Length;
                    archivo.FechaModificacion = DateTime.Now;

                    GuardarFatTable();
                    Console.WriteLine("Archivo modificado exitosamente.");
                }
                else
                {
                    Console.WriteLine("Modificación cancelada.");
                }
            }
        }
        else
        {
            Console.WriteLine("Selección no válida.");
        }
    }

    static void EliminarArchivo()
    {
        ListarArchivos(false);
        Console.Write("\nSeleccione el número del archivo que desea eliminar: ");
        if (int.TryParse(Console.ReadLine(), out int index) && index > 0 && index <= fatTable.Count)
        {
            var archivo = fatTable[index - 1];
            Console.Write($"¿Está seguro que desea eliminar el archivo '{archivo.Nombre}'? (S/N): ");
            if (Console.ReadLine().Trim().ToUpper() == "S")
            {
                archivo.EnPapelera = true;
                archivo.FechaEliminacion = DateTime.Now;

                GuardarFatTable();
                Console.WriteLine("Archivo movido a la papelera de reciclaje.");
            }
        }
        else
        {
            Console.WriteLine("Selección no válida.");
        }
    }

    static void RecuperarArchivo()
    {
        ListarArchivos(true);
        Console.Write("\nSeleccione el número del archivo que desea recuperar: ");
        if (int.TryParse(Console.ReadLine(), out int index) && index > 0 && index <= fatTable.Count)
        {
            var archivo = fatTable[index - 1];
            Console.Write($"¿Está seguro que desea recuperar el archivo '{archivo.Nombre}'? (S/N): ");
            if (Console.ReadLine().Trim().ToUpper() == "S")
            {
                archivo.EnPapelera = false;
                archivo.FechaEliminacion = null;

                GuardarFatTable();
                Console.WriteLine("Archivo recuperado exitosamente.");
            }
        }
        else
        {
            Console.WriteLine("Selección no válida.");
        }
    }

    static List<string> SegmentarDatos(string datos, int maxSize)
    {
        List<string> segmentos = new List<string>();
        for (int i = 0; i < datos.Length; i += maxSize)
        {
            segmentos.Add(datos.Substring(i, Math.Min(maxSize, datos.Length - i)));
        }
        return segmentos;
    }

    static string GuardarSegmentos(List<string> segmentos)
    {
        string siguienteArchivoPath = null;
        for (int i = segmentos.Count - 1; i >= 0; i--)
        {
            string segmentoFilePath = Path.Combine(folderPath, $"{Guid.NewGuid()}.json");

            SegmentoDatos segmentoDatos = new SegmentoDatos
            {
                Datos = segmentos[i],
                SiguienteArchivo = siguienteArchivoPath,
                EOF = (i == segmentos.Count - 1)
            };

            string jsonString = JsonSerializer.Serialize(segmentoDatos);
            File.WriteAllText(segmentoFilePath, jsonString);

            siguienteArchivoPath = segmentoFilePath;
        }
        return siguienteArchivoPath;
    }

    static string LeerArchivoCompleto(string rutaInicial)
    {
        string contenido = "";
        string siguienteArchivo = rutaInicial;

        while (!string.IsNullOrEmpty(siguienteArchivo))
        {
            string json = File.ReadAllText(siguienteArchivo);
            SegmentoDatos segmento = JsonSerializer.Deserialize<SegmentoDatos>(json);

            contenido += segmento.Datos;
            siguienteArchivo = segmento.SiguienteArchivo;
        }

        return contenido;
    }

    static void EliminarArchivosSegmentados(string rutaInicial)
    {
        string siguienteArchivo = rutaInicial;

        while (!string.IsNullOrEmpty(siguienteArchivo))
        {
            string json = File.ReadAllText(siguienteArchivo);
            SegmentoDatos segmento = JsonSerializer.Deserialize<SegmentoDatos>(json);

            
            File.Delete(siguienteArchivo);
            siguienteArchivo = segmento.SiguienteArchivo;
        }
    }

    static void GuardarFatTable()
    {
        string jsonString = JsonSerializer.Serialize(fatTable);
        File.WriteAllText(fatTablePath, jsonString);
    }

    static string LeerEntrada()
    {
        string input = "";
        ConsoleKey key;
        do
        {
            var keyInfo = Console.ReadKey(intercept: true);
            key = keyInfo.Key;

            if (key == ConsoleKey.Escape) break;
            input += keyInfo.KeyChar;
        }
        while (key != ConsoleKey.Escape);

        return input;
    }
}

class FatTableEntry
{
    public string Nombre { get; set; }
    public string RutaInicial { get; set; }
    public bool EnPapelera { get; set; }
    public int TotalCaracteres { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaModificacion { get; set; }
    public DateTime? FechaEliminacion { get; set; }
}

class SegmentoDatos
{
    public string Datos { get; set; }
    public string SiguienteArchivo { get; set; }
    public bool EOF { get; set; }
}
