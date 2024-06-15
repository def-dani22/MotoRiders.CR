/* ------------------------------------ Click on login and Sign Up to  changue and view the effect
---------------------------------------
*/

const time_to_show_login = 400;
const time_to_hidden_login = 200;

function change_to_login() {
    document.querySelector('.cont_forms').className = "cont_forms cont_forms_active_login";
    document.querySelector('.cont_form_login').style.display = "block";
    document.querySelector('.cont_form_sign_up').style.opacity = "0";

    setTimeout(function () { document.querySelector('.cont_form_login').style.opacity = "1"; }, time_to_show_login);

    setTimeout(function () {
        document.querySelector('.cont_form_sign_up').style.display = "none";
    }, time_to_hidden_login);
}

const time_to_show_sign_up = 100;
const time_to_hidden_sign_up = 400;

function change_to_sign_up(at) {
    document.querySelector('.cont_forms').className = "cont_forms cont_forms_active_sign_up";
    document.querySelector('.cont_form_sign_up').style.display = "block";
    document.querySelector('.cont_form_login').style.opacity = "0";

    setTimeout(function () {
        document.querySelector('.cont_form_sign_up').style.opacity = "1";
    }, time_to_show_sign_up);

    setTimeout(function () {
        document.querySelector('.cont_form_login').style.display = "none";
    }, time_to_hidden_sign_up);


}

const time_to_hidden_all = 500;

function hidden_login_and_sign_up() {

    document.querySelector('.cont_forms').className = "cont_forms";
    document.querySelector('.cont_form_sign_up').style.opacity = "0";
    document.querySelector('.cont_form_login').style.opacity = "0";

    setTimeout(function () {
        document.querySelector('.cont_form_sign_up').style.display = "none";
        document.querySelector('.cont_form_login').style.display = "none";
    }, time_to_hidden_all);

}

// Obtener todos los botones
const buttons = document.querySelectorAll('.buttonC');

// Agregar un listener de clic al documento para cerrar los tooltips al hacer clic fuera del botón
document.addEventListener('click', function (event) {
    buttons.forEach(button => {
        // Verificar si el clic no ocurrió dentro del botón o su tooltip
        if (!button.contains(event.target)) {
            // Ocultar el tooltip del botón
            button.querySelector('.buttonC::before').style.opacity = 0;
            button.querySelector('.buttonC::before').style.visibility = 'hidden';
        }
    });
});



/*sssssss*/


// Función para desmarcar todos los checkboxes
function uncheckAllCheckboxes() {
    var checkboxes = document.querySelectorAll('.form-check-input');
    checkboxes.forEach(function (checkbox) {
        checkbox.checked = false;
    });
}

// Event listener para el botón "Agregar al Carrito"
document.querySelector('.buttonC').addEventListener('click', function () {
    // Llamar a la función para desmarcar todos los checkboxes
    uncheckAllCheckboxes();

    // Aquí puedes agregar la lógica adicional para agregar elementos al carrito
    // Por ejemplo, podrías obtener los IDs de las motocicletas seleccionadas
    // y enviarlos al servidor para procesar la compra.

    // También puedes mostrar algún tipo de confirmación al usuario de que los
    // productos fueron agregados al carrito.

    // Por ejemplo, mostrar un tooltip o mensaje de éxito usando CSS y JavaScript.
    var tooltip = document.querySelector('.buttonC');
    tooltip.setAttribute('data-tooltip', 'Agregado al carrito');
    setTimeout(function () {
        tooltip.setAttribute('data-tooltip', 'Agregado');
    }, 1500); // Cambia el tooltip de vuelta después de 1.5 segundos
});

// Función para cerrar el tooltip después de un tiempo
setTimeout(function () {
    document.querySelector('.buttonC').setAttribute('data-tooltip', 'Agregado');
}, 3000)