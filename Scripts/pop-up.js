$(document).ready(function() {
  var maxZoom = 8;

 var defaultOptions = {
  message: null,
  title: null,
  okCallback: function() {},
  okText: "OK",
  cancelCallback: null,
  cancelText: "Cancel",
  image: null
 };

 var okSel = "div.pop-up div.buttons input[name='ok']";
 var cancelSel = "div.pop-up div.buttons input[name='cancel']";
 var currentPopups = [];

 var currentPopup = function(){
  if (currentPopups.length == 0)
   return null;
 
  return currentPopups[currentPopups.length - 1];
 }

 var displayPopup = function() {
  var options = currentPopup();
  if (!options)
   return hidePopup();

  var message = options.message.replace(/\n/g, "<br />");
  if (options.image)
   message = "<img src=\"" + options.image + "\" />" + message;

  $(okSel).val(options.okText);
  $(cancelSel).val(options.cancelText);
  options.okCallback ? $(okSel).show() : $(okSel).hide();
  options.cancelCallback ? $(cancelSel).show() : $(cancelSel).hide();
  $("div.pop-up h2").html(options.title);
  $("div.pop-up div.scrolling-content").html(message);
  
  showPopup();
  
  $("div.popup").css("margin-top", $("div.popup").prop("height") / -2)
 }

 var invokeCallbackAndClose = function(callback) {
  if (callback)
   callback();

  currentPopups.pop();
  displayPopup();
 }

 var getWindowSize = function(){
  return {
	width: $(window).width(),
	height: $(window).height()
  };
 };

 var showPopup = function() {
  var windowSize = getWindowSize();

  $("div.pop-up-container").width(windowSize.width);
  $("div.pop-up-container").height(windowSize.height);
  setPopupStyles($("div.pop-up"), null, windowSize);

 /* if (!confirm("document.body.clientHeight: " + document.body.clientWidth + "x" + document.body.clientHeight +
        "\nscreen.height: " + screen.width + "x" + screen.height + 
        "\n$(window).height(): " + $(window).width() + "x" + $(window).height() +
        "\nwindow.scrollMaxY: " + window.scrollMaxY +
        "\nwindow.scrollY: " + window.scrollY +
        "\ndocument.documentElememt.offsetY: " + document.documentElement.offsetY))
    return false; */

  $("div.pop-up-container").show();
 }

 var setPopupStyles = function(popup, zoom, windowSize) {
  popup.css("top", "50%");
  popup.css("left", "50%");
 }

 var hidePopup = function() {
  $("div.pop-up-container").hide();
 }

 $(okSel).click(function() {
  if (!currentPopup())
   hidePopup();

  invokeCallbackAndClose(currentPopup().okCallback);
 });

  $(cancelSel).click(function() {
   if (!currentPopup())
    hidePopup();

  invokeCallbackAndClose(currentPopup().cancelCallback);
 });

 window.domMessage = function(options) {
  currentPopups.push($.extend({}, defaultOptions, options));
  displayPopupBlocking();
 }

 var displayPopupBlocking = function(){
  var options = currentPopup();
  if (!options.cancelCallback) {
	displayPopup();
  }
  else {
	displayPopup();
  }
 }
});