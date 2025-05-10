using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Drawing;

namespace TAREAPRO
{
    public partial class Form1 : Form
    {
        // Variable global para la conexión HTTP
        private static readonly HttpClient client = new HttpClient();

        public Form1()
        {
            InitializeComponent();
        }

        // Evento para el botón de Buscar
        private async void btnBuscar_Click(object sender, EventArgs e)
        {
            try
            {
                lblEstado.Text = "Cargando...";
                string pokemonName = txtNombre.Text.ToLower().Trim();

                // Verificar si el campo de nombre está vacío
                if (string.IsNullOrWhiteSpace(pokemonName))
                {
                    lblEstado.Text = "Por favor, ingrese un nombre.";
                    return;
                }

                string url = $"https://pokeapi.co/api/v2/pokemon/{pokemonName}";
                lblEstado.Text = $"Consultando: {url}";

                HttpResponseMessage response = await client.GetAsync(url);

                // Verificar si la respuesta fue exitosa
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    var pokemon = JsonConvert.DeserializeObject<Pokemon>(data);

                    // Mostrar los datos obtenidos en la interfaz
                    txtInfo.Text = $"Nombre: {pokemon.Name}\nAltura: {pokemon.Height / 10.0} m\nPeso: {pokemon.Weight / 10.0} kg";

                    // Cargar la imagen
                    if (!string.IsNullOrEmpty(pokemon.Sprites.FrontDefault))
                    {
                        var imageUrl = pokemon.Sprites.FrontDefault;
                        pbPokemon.Image = await LoadImageAsync(imageUrl);  // Cargar imagen de forma asíncrona
                        lblEstado.Text = "Datos cargados correctamente.";
                    }
                    else
                    {
                        lblEstado.Text = "No se encontró la imagen.";
                    }
                }
                else
                {
                    lblEstado.Text = $"Pokémon no encontrado. Código de estado: {response.StatusCode}";
                }
            }
            catch (HttpRequestException ex)
            {
                lblEstado.Text = "Error de conexión: " + ex.Message;
            }
            catch (JsonException ex)
            {
                lblEstado.Text = "Error al procesar los datos: " + ex.Message;
            }
            catch (Exception ex)
            {
                lblEstado.Text = "Error inesperado: " + ex.Message;
            }
        }

        // Método para cargar la imagen de manera asíncrona
        private async Task<Image> LoadImageAsync(string imageUrl)
        {
            try
            {
                using (var stream = await client.GetStreamAsync(imageUrl))
                {
                    return Image.FromStream(stream);
                }
            }
            catch (Exception ex)
            {
                // Si no se puede cargar la imagen, mostrar un error
                lblEstado.Text = "Error al cargar la imagen: " + ex.Message;
                return null;
            }
        }

        // Evento para el botón Limpiar
        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            txtNombre.Clear();
            txtInfo.Clear();
            pbPokemon.Image = null;
            lblEstado.Text = "";
        }
    }

    // Clase para mapear la respuesta JSON de la API
    public class Pokemon
    {
        public string Name { get; set; }
        public int Height { get; set; } // La altura viene en decímetros, se divide por 10 para mostrar en metros
        public int Weight { get; set; } // El peso viene en hectogramos, se divide por 10 para mostrar en kilogramos
        public Sprites Sprites { get; set; }
    }

    public class Sprites
    {
        public string FrontDefault { get; set; }
    }
}
