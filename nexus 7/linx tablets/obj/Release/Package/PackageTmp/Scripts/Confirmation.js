       function confirmLeadTimePurge() {
            var confirm_value = document.createElement("INPUT");
            confirm_value.type = "hidden";
            confirm_value.name = "confirm_value";
            if (confirm("Please confirm if you wish to remove all existing product lead times, cannot be undone")) 
            {
            
                
                confirm_value.value = "Yes";
            
            }
             else {
                confirm_value.value = "No";
            }
            document.forms[0].appendChild(confirm_value);
        }
       function confirmReplenDelete() {
           var confirm_value = document.createElement("INPUT");
           confirm_value.type = "hidden";
           confirm_value.name = "confirm_value";
           if (confirm("Please confirm if you wish to remove this replen, cannot be undone!")) {


               confirm_value.value = "Yes";

           }
           else {
               confirm_value.value = "No";
           }
           document.forms[0].appendChild(confirm_value);
       }
        
        
        