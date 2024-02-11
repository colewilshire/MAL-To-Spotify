mergeInto(LibraryManager.library, {
  // Function to log document.referrer to the console
  LogDocumentReferrer: function() {
    console.log("Referrer URL:", document.referrer);
  }
});