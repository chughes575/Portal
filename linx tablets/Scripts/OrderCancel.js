function switchViews (obj)
    {
        var div = document.getElementById(obj);
        var img = document.getElementById('img' + obj);
        
        if (div.style.display=="none")
            {
                div.style.display = "inline";
                img.src="../images/Expand_button_down.png";               
            }        
        else
            {
                div.style.display = "none";                               
                img.src="../images/Expand_Button.png";           
               
            }
    }
    function Confirm() {
            var confirm_value = document.createElement("INPUT");
            confirm_value.type = "hidden";
            confirm_value.name = "confirm_value";
            if (confirm("Do you want to delete the plu records?")) 
            {
            
                 if (confirm("Do you definetely want to remove these records, this is permanent and the records deleted cannot be retrieved?")) 
                 {
                confirm_value.value = "Yes";
                 } 
                 else 
                 {
                confirm_value.value = "No";
                  }
            
            }
             else {
                confirm_value.value = "No";
            }
            document.forms[0].appendChild(confirm_value);
        }
        
        function ConfirmMessage(Message) {
            var confirm_value = document.createElement("INPUT");
            confirm_value.type = "hidden";
            confirm_value.name = "confirm_value";
            if (confirm(Message)) 
            {
            
                
                confirm_value.value = "Yes";
            
            }
             else {
                confirm_value.value = "No";
            }
            document.forms[0].appendChild(confirm_value);
        }
        
        function Confirm1() {
            var confirm_value = document.createElement("INPUT");
            confirm_value.type = "hidden";
            confirm_value.name = "confirm_value";
            if (confirm("Do you want to delete the plu records?")) 
            {
            
                 if (confirm("Do you definetely want to remove these records, this is permanent and cannot be retrieved?")) 
                 {
                confirm_value.value = "Yes";
                 } 
                 else 
                 {
                confirm_value.value = "No";
                  }
            
            }
             else {
                confirm_value.value = "No";
            }
            document.forms[0].appendChild(confirm_value);
        }
        
          function Confirm2() {
            var confirm_value = document.createElement("INPUT");
            confirm_value.type = "hidden";
            confirm_value.name = "confirm_value";
            if (confirm("Do you want to modfy the failed PLU records?")) 
            {
            
                confirm_value.value = "Yes";
            
            }
             else {
                confirm_value.value = "No";
            }
            document.forms[0].appendChild(confirm_value);
            
            
        }
		function ConfirmSupplierStock() {
            var confirm_value = document.createElement("INPUT");
            confirm_value.type = "hidden";
            confirm_value.name = "confirm_value";
            if (confirm("Do you want to delete supplier stock?")) 
            {
            
                confirm_value.value = "Yes";
            
            }
             else {
                confirm_value.value = "No";
            }
            document.forms[0].appendChild(confirm_value);
			}
			function ConfirmSupplierStock() {
            var confirm_value = document.createElement("INPUT");
            confirm_value.type = "hidden";
            confirm_value.name = "confirm_value";
            if (confirm("Do you want to delete supplier stock?")) 
            {
            
                 if (confirm("Do you definetely want to remove these records, this is permanent and cannot be retrieved?")) 
                 {
                confirm_value.value = "Yes";
                 } 
                 else 
                 {
                confirm_value.value = "No";
                  }
            
            }
             else {
                confirm_value.value = "No";
            }
            document.forms[0].appendChild(confirm_value);
        }

 function ConfirmPromotion() {
            var confirm_value = document.createElement("INPUT");
            confirm_value.type = "hidden";
            confirm_value.name = "confirm_value";
            if (confirm("Please confirm if you wish to delete this promotion? (*this will also delete all associated promotional products and prices")) 
            {
            
                
                confirm_value.value = "Yes";
            
            }
             else {
                confirm_value.value = "No";
            }
            document.forms[0].appendChild(confirm_value);
        }
         function confirmPOApproval() {
            var confirm_value = document.createElement("INPUT");
            confirm_value.type = "hidden";
            confirm_value.name = "confirm_value";
            if (confirm("Please confirm if you wish to confirm this PO? (*this will action this PO and make it live")) 
            {
            
                
                confirm_value.value = "Yes";
            
            }
             else {
                confirm_value.value = "No";
            }
            document.forms[0].appendChild(confirm_value);
        }
        function confirmPORejection() {
            var confirm_value = document.createElement("INPUT");
            confirm_value.type = "hidden";
            confirm_value.name = "confirm_value";
            if (confirm("Please confirm if you wish to reject this PO? (*this will action this PO and prevent it from being submitted to macs, this can later be undone and made live)")) 
            {
            
                
                confirm_value.value = "Yes";
            
            }
             else {
                confirm_value.value = "No";
            }
            document.forms[0].appendChild(confirm_value);
        }
		function confirmOrderRelease() {
            var confirm_value = document.createElement("INPUT");
            confirm_value.type = "hidden";
            confirm_value.name = "confirm_value";
            if (confirm("Please confirm if you wish to approve/release all held lines on this order? (this will then submit the order to oracale and cannot be undone)")) 
            {
            
                
                confirm_value.value = "Yes";
            
            }
             else {
                confirm_value.value = "No";
            }
            document.forms[0].appendChild(confirm_value);
        }
        function confirmOrderLineRelease() {
            var confirm_value = document.createElement("INPUT");
            confirm_value.type = "hidden";
            confirm_value.name = "confirm_value";
            if (confirm("Please confirm if you wish to approve/release this order line? (this will then submit the order line to oracale and cannot be undone)")) 
            {
            
                
                confirm_value.value = "Yes";
            
            }
             else {
                confirm_value.value = "No";
            }
            document.forms[0].appendChild(confirm_value);
        }
        function confirmProductOrderRelease() {
            var confirm_value = document.createElement("INPUT");
            confirm_value.type = "hidden";
            confirm_value.name = "confirm_value";
            if (confirm("Please confirm if you wish to approve/release all held orders that contain this product? (this will then submit the order line to oracale and cannot be undone)")) 
            {
            
                
                confirm_value.value = "Yes";
            
            }
             else {
                confirm_value.value = "No";
            }
            document.forms[0].appendChild(confirm_value);
        }
        
        
        
        