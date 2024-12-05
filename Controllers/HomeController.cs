using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using Microsoft.Reporting.WebForms;
using Reporteadores.Models;
using System.Drawing;
using System;
using Microsoft.CodeAnalysis.RulesetToEditorconfig;
using Microsoft.ReportingServices.ReportProcessing.ReportObjectModel;

namespace Reporteadores.Controllers
{
    public class HomeController : Controller
    {
        private readonly BarronContext _BContext = new BarronContext();
        private readonly ComparteContext _CContext = new ComparteContext();
        static string reporteDireccion { get; set; }
        private readonly ILogger<HomeController> _logger;

        //public HomeController(ComparteContext Ccontext, BarronContext BContext, ILogger<HomeController> logger) { 

        //[Route("[controller]/[action]")]
        public HomeController(ILogger<HomeController> logger) => _logger = logger;

        public IActionResult Index()
        {
            var usernameSesion = HttpContext.Session.GetString("Username"); // Recupera el nombre de usuario de la sesión

            if (usernameSesion != null)
            {
                // El usuario está autenticado, realiza la lógica deseada
                ViewData["UsuarioActivo"] = usernameSesion;
                return View();
            }
            else
            {
                // El usuario no está autenticado, redirigir a la página de inicio de sesión
                return RedirectToAction("Login", "Account");
            }
        }

        public IActionResult Privacy()
        {
            string pdfUrl = "http://189.203.75.86/Kioscos/SitioRecibos/Aviso.pdf";
            return Redirect(pdfUrl); // Redirige al navegador para que abra el PDF
        }

        public IActionResult Reporte()
        {
            var username = HttpContext.Session.GetString("Username"); // Recupera el nombre de usuario de la sesión

            if (username != null)
            {
                var usuario = _BContext.Reportes.Select(u => new { u.ReNombre });

                // Llenar el primer ComboBox con los años únicos
                var periodos = _BContext.Periodos.Select(p => p.PeYear).Distinct().ToList();
                ViewBag.PeAnio = periodos;
                var reportes = _BContext.Reportes.ToList();
                ViewBag.Reporte = reportes;
                ViewData["UsuarioActivo"] = username;
                if (usuario != null)
                    return View();
                else
                    return RedirectToAction("Error", "Home");
            }
            return RedirectToAction("Login", "Account");
        }

        public IActionResult Signout()
        {
            // no se que paso aqui pero funciona 
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");            
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult GenerarReporte()
        {
            // Ruta del archivo RDLC
            string reportPath = Path.Combine(Directory.GetCurrentDirectory(), "Reports", "MiReporte.rdlc");

            // Carga el archivo RDLC
            var localReport = new LocalReport();
            localReport.LoadReportDefinition(System.IO.File.OpenRead(reportPath));

            // Agregar datos al informe
            // Nota: "NombreDelDataSet" debe coincidir con el nombre del DataSet configurado en el RDLC
            /*var datos = ObtenerDatos(); // Método que devuelve los datos del informe
            var dataSourceNames = localReport.GetDataSourceNames();
            var dataSource = new ReportDataSource(dataSourceNames[0].ToString(), datos);
            localReport.DataSources.Add(dataSource);*/

            // Renderizar el informe como PDF
            var pdfBytes = localReport.Render("PDF");

            // Devolver el archivo PDF al cliente
            return File(pdfBytes, "application/pdf", "Reporte.pdf");
        }

        public string GetSqlQueryFromRdl(string rdlPath)
        {
            // Leer todo el contenido del archivo
            string content = null;
            string sqlQuery = null;
            try
            {
                //content = File.ReadAllText(rdlPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error leyendo el archivo: {ex.Message}");
                return null;
            }

            // Usar una expresión regular para encontrar el texto entre las etiquetas <CommandText> y </CommandText>
            string pattern = @"<CommandText>(.*?)<\/CommandText>";
            Match match = Regex.Match(content, pattern, RegexOptions.Singleline);

            if (match.Success)
            {
                sqlQuery = match.Groups[1].Value.Trim();
                Console.WriteLine("Consulta SQL extraída:");
                Console.WriteLine(sqlQuery);
            }
            else
            {
                Console.WriteLine("No se encontró ninguna consulta SQL entre las etiquetas <CommandText>.");
            }
            return sqlQuery;
        }

        // Método para obtener los tipos (PeTipo) según el año seleccionado (PeAnio)
        [HttpGet]
        public JsonResult GetPeTipos(int peAnio)
        {
            var tipos = _BContext.Periodos
                .Where(p => p.PeYear == peAnio)
                .Select(p => p.PeTipo)
                .Distinct()
                .ToList();

            try
            { 
                return Json(tipos); 
            } 
            catch (Exception ex) 
            { 
                return Json(new { error = ex.Message }); 
            }
              
        }

        // Método para obtener los números de periodo (PeNumero) según el tipo seleccionado (PeTipo)
        [HttpGet]
        public JsonResult GetTeNumeroPeriodos(int peAnio,int peTipo)
        {
            var numeros = _BContext.Periodos
                .Where(p => p.PeTipo == peTipo && p.PeYear == peAnio) // Filtra por Tipo y Año
                .Select(p => p.PeNumero) // Selecciona el número
                .Distinct() // Elimina duplicados
                .OrderBy(p => p) // Ordena de menor a mayor
                .ToList(); // Convierte a lista
            return Json(numeros);
        }

        
        public IActionResult createreports(string ReCodigo, string ReNombre, string peTipo, string peAnio, string peNumero)
        {
            //repRLDC = new CreatorReportsCustoms();
            

            var rutaReporte = _BContext.Reportes.Where(u => u.ReNombre == ReNombre &&  u.ReCodigo == short.Parse(ReCodigo))
                                        .Select(u => new { u.ReArchivo})  
                                        .FirstOrDefault();


            if (rutaReporte != null)
            {
                reporteDireccion = rutaReporte.ReArchivo;
            }
            else
            {
                //reporteDireccion = reporteDireccion + TituloText.Text + ".rdlc";
            }

               
            try
            {
                /*
                formReports = new VentanaReportesRDLC();
                string activo = "Si";
                LocalReport.ReportPath = reporteDireccion;

                if (File.Exists(reporteDireccion))
                {
                    // El reporte está listo para abrirse
                    if (MessageBox.Show("¿Quieres usar los parametros predeterminados?", "¡Parametros!", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {

                        if (Activo.Checked)
                        {
                            activo = "S";
                        }
                        else
                        {
                            activo = "N";
                        }
                        //vDatosPeriodo:
                        //[0]Año, [1]Tipo(Numero), [2]Numero, [3]Empleado, [4]Tipo(Texto), [5]Fecha Inicio, [6]Fecha Fin, [7]Nombre Empleado

                        List<ReportParameter> parametros = new List<ReportParameter>
                            {
                            new ReportParameter("EMPRESA", "BARRON"),
                            new ReportParameter("REPORTE", TituloText.Text),
                            new ReportParameter("ACTIVO", activo),
                            new ReportParameter("FECHAINI", FechaIni.Text),
                            new ReportParameter("FECHAFIN", FechaFin.Text),
                            new ReportParameter("AÑO", nav.anio),
                            new ReportParameter("PERIODO", nav.numNom),
                            new ReportParameter("TIPO", nav.tipoNom),
                            new ReportParameter("EMPLEADO", nav.IdCodigoColabora)
                        };

                        SetReportParameters(formReports.reportViewer1, parametros);
                    }

                    var parameters = formReports.reportViewer1.LocalReport.GetParameters();
                    parametrosView(true);
                    Label myLabel = new Label();
                    myLabel.Size = new Size(150, 23);
                    //Microsoft Sans Serif, 16pt, style=Bold
                    myLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 16, FontStyle.Bold);
                    myLabel.Text = "Parametros";
                    flowLayoutPanel1.Controls.Add(myLabel);
                    // Forzar un salto de línea después de este Label
                    flowLayoutPanel1.SetFlowBreak(myLabel, true);
                    foreach (var param in parameters)
                    {

                        Label label = new Label();
                        label.Size = new Size(150, 23);
                        myLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10);
                        label.Text = param.Prompt;
                        flowLayoutPanel1.Controls.Add(label);

                        TextBox textBox = new TextBox();
                        textBox.Size = new Size(150, 23);
                        textBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8);
                        textBox.Name = "textBox" + param.Name; // Nombre del TextBox basado en el nombre del parámetro
                        if (param.Values.Count != 0)
                        {
                            if (param.Values[0] != null)
                                textBox.Text = param.Values[0].ToString();
                        }
                        flowLayoutPanel1.Controls.Add(textBox);
                        flowLayoutPanel1.SetFlowBreak(textBox, true);
                    }
                    //formReports.reportViewer1.LocalReport.DataSources.Clear();

                    //formReports.reportViewer1.LocalReport.DataSources[0] = reportDataSource;
                    //añede los controles al reporte
                    //formReports.Controls.Add(formReports.reportViewer1);



                    Button submitButton = new Button();
                    submitButton.Text = "Generar Reporte";
                    submitButton.Click += new EventHandler(submitButton_Click);
                    flowLayoutPanel1.Controls.Add(submitButton);
                }
                else
                {
                    MessageBox.Show("El reporte no está listo para abrirse, revisa la ruta del archivo.");
                }

                */
            }
            catch (FileNotFoundException ex)
            {
                //MessageBox.Show("Error: " + ex.Message);
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
