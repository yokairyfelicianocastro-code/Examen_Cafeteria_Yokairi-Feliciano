using System;
using System.Globalization;
using System.Windows.Forms;

namespace Exmen_Cafeteria_YokairiFeliciano
{
    public partial class Form1 : Form
    {
        // mis variables y precios fijos para el examen
        private const decimal RECARGO_GRANDE = 25m;
        private const decimal RECARGO_EXTRA = 30m;
        private const decimal DESC_FRECUENTE = 0.10m;
        private const decimal COSTO_EMPAQUE = 20m;
        private const decimal TASA_ITBIS = 0.18m;

        // para el formato de RD$ de aqui
        private readonly CultureInfo rdCulture = new CultureInfo("es-DO");

        // aqui guardo lo que se va sumando
        private decimal subtotalPedido = 0m;

        public Form1()
        {
            InitializeComponent();

            // configuro de una vez para que inicie limpio
            this.Load += Form1_Load;

            // vinculo los botones con sus acciones
            btnAgregar.Click += btnAgregar_Click;
            btnCalcular.Click += btnCalcular_Click;
            btnLimpiar.Click += btnLimpiar_Click;
            btnSalir.Click += btnSalir_Click;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InicializarFormulario();
        }

        // resetea todo a como debe empezar
        private void InicializarFormulario()
        {
            txtCliente.Clear();
            dtpFecha.Value = DateTime.Today;
            chkFrecuente.Checked = false;

            LimpiarCamposProducto();

            rbtLocal.Checked = true;
            rbtLlevar.Checked = false;

            lstPedido.Items.Clear();
            subtotalPedido = 0m;

            // el encabezado con los espacios exactos
            lblEncabezado.Text = FormatearFila("Cant", "Producto", "Precio", "Total");

            PonerTotalesEnCero();
        }

        // borra solo los datos del producto que se acaba de agregar
        private void LimpiarCamposProducto()
        {
            txtProducto.Clear();
            txtPrecio.Clear();
            nudCantidad.Value = 1;
            rbtNormal.Checked = true;
            rbtGrande.Checked = false;
            chkPorcionExtra.Checked = false;
        }

        private void PonerTotalesEnCero()
        {
            string cero = FormatearRd(0m);
            lblSubtotal.Text = cero;
            lblDescuento.Text = cero;
            lblEmpaque.Text = cero;
            lblItbis.Text = cero;
            lblTotal.Text = cero;
        }

        // cuando le doy a agregar el producto a la lista
        private void btnAgregar_Click(object sender, EventArgs e)
        {
            string prod = txtProducto.Text.Trim();

            // que no dejen el nombre vacio
            if (string.IsNullOrWhiteSpace(prod))
            {
                MessageBox.Show("Pon el nombre del producto.", "Atencion",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtProducto.Focus();
                return;
            }

            // intento leer el precio, si ponen letras avisa
            if (!decimal.TryParse(txtPrecio.Text.Trim(), NumberStyles.Number, rdCulture, out decimal precioBase) &&
                !decimal.TryParse(txtPrecio.Text.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out precioBase))
            {
                MessageBox.Show("Pon un precio valido.", "Error de precio",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPrecio.Focus();
                return;
            }

            if (precioBase <= 0)
            {
                MessageBox.Show("El precio debe ser mayor a 0.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPrecio.Focus();
                return;
            }

            // calculo el precio de este producto con sus extras
            decimal precioFinal = precioBase;
            if (rbtGrande.Checked) precioFinal += RECARGO_GRANDE;
            if (chkPorcionExtra.Checked) precioFinal += RECARGO_EXTRA;

            int cant = (int)nudCantidad.Value;
            decimal totalFila = precioFinal * cant;

            // armo el nombre para que se vea claro que lleva
            string tam = rbtGrande.Checked ? "Grande" : "Normal";
            string detalle = chkPorcionExtra.Checked
                ? $"{prod} ({tam}, Extra)"
                : $"{prod} ({tam})";

            // armo la fila bien espaciada
            string nuevaFila = FormatearFila(
                cant.ToString(),
                detalle,
                precioFinal.ToString("0.00", CultureInfo.InvariantCulture),
                totalFila.ToString("0.00", CultureInfo.InvariantCulture)
            );

            // agrego y acumulo
            lstPedido.Items.Add(nuevaFila);
            subtotalPedido += totalFila;

            // limpio para el siguiente producto
            LimpiarCamposProducto();
            txtProducto.Focus();
        }

        // cuando le doy a calcular todo el pedido
        private void btnCalcular_Click(object sender, EventArgs e)
        {
            if (lstPedido.Items.Count == 0)
            {
                MessageBox.Show("Primero agrega productos al pedido.", "Sin productos",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // descuento de frecuente 10%
            decimal descuento = chkFrecuente.Checked ? subtotalPedido * DESC_FRECUENTE : 0m;

            // si es para llevar sumamos 20 pesos
            decimal empaque = rbtLlevar.Checked ? COSTO_EMPAQUE : 0m;

            // base para el ITBIS
            decimal baseImponible = subtotalPedido - descuento + empaque;
            decimal itbis = baseImponible * TASA_ITBIS;
            decimal granTotal = baseImponible + itbis;

            // muestro los resultados en pantalla
            lblSubtotal.Text = FormatearRd(subtotalPedido);
            lblDescuento.Text = FormatearRd(descuento);
            lblEmpaque.Text = FormatearRd(empaque);
            lblItbis.Text = FormatearRd(itbis);
            lblTotal.Text = FormatearRd(granTotal);
        }

        // boton para empezar de nuevo
        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            InicializarFormulario();
            txtCliente.Focus();
        }

        // boton para cerrar el examen
        private void btnSalir_Click(object sender, EventArgs e)
        {
            DialogResult opcion = MessageBox.Show("¿Quieres salir de la aplicacion?", "Salir",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (opcion == DialogResult.Yes)
            {
                this.Close();
            }
        }

        // ajusta los espacios para que todo caiga en su columna
        private string FormatearFila(string cant, string producto, string precio, string total)
        {
            if (producto.Length > 24)
                producto = producto.Substring(0, 24);

            return cant.PadRight(6) +
                   producto.PadRight(24) +
                   precio.PadLeft(10) +
                   total.PadLeft(12);
        }

        // pone el prefijo RD$
        private string FormatearRd(decimal valor)
        {
            return "RD$ " + valor.ToString("0.00", CultureInfo.InvariantCulture);
        }
    }
}


