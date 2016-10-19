// Decompiled with JetBrains decompiler
// Type: linx_tablets.CustomerService.AppleReporting
// Assembly: linx-tablets, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 79E24B2C-4AF9-4E0E-BA5E-3953F1521489
// Assembly location: C:\apple site\Apple Reporting\WebApplication1\bin\linx-tablets.dll

using MSE_Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;

namespace linx_tablets.CustomerService
{
  public partial class AppleReporting : Page
  {

    protected void Page_Load(object sender, EventArgs e)
    {

         if (HttpContext.Current.User.Identity.Name.ToString() == "appleadmin1")
      {
        this.pnlAdmin.Visible = true;
        this.oustandingFileCounts();
      }
      if (this.IsPostBack)
        return;
    }

    private void runReport(string query, string filename)
    {
      this.Session["ReportQuery"] = (object) query;
      this.Session["ReportQueryIsSp"] = (object) false;
      this.Session["ReportDelimiter"] = (object) ",";
      this.Session["ReportHasHeader"] = (object) true;
      this.Session["ReportFileName"] = (object) filename;
      this.Session["ReportTextQualifier"] = (object) "\"";
      this.Response.Redirect("~/reporting/report-export-csv.aspx");
    }

    

    private void processInventoryFiles()
    {
      string path1 = "\\\\10.16.72.129\\company\\FTP\\root\\MSESRVDOM\\apple\\In\\Apple Inventory\\IT\\";
      string path2 = "\\\\10.16.72.129\\company\\FTP\\root\\MSESRVDOM\\apple\\In\\Apple Inventory\\UK\\";
      string path3 = "\\\\10.16.72.129\\company\\FTP\\root\\MSESRVDOM\\apple\\In\\Apple Inventory\\UAE\\";
      string path4 = "\\\\10.16.72.129\\company\\FTP\\root\\MSESRVDOM\\apple\\In\\Apple Inventory\\NL\\";
      try
      {
        foreach (string str1 in new ArrayList((ICollection) Directory.GetFiles(path2)))
        {
          string contents = Regex.Replace(System.IO.File.ReadAllText(str1, Encoding.Default), ",(?=[^\"]*\"(?:[^\"]*\"[^\"]*\")*[^\"]*$)", string.Empty);
          string str2 = Path.GetFileNameWithoutExtension(str1) + "_amended" + Common.timestamp() + ".csv";
          try
          {
            System.IO.File.AppendAllText(path2 + "\\tempfiles\\" + str2, contents, Encoding.Default);
            System.IO.File.Move(path2 + "\\tempfiles\\" + str2, path2 + "\\processed\\" + str2);
            if (System.IO.File.Exists(path2 + "\\original files\\" + Path.GetFileName(str1)))
              System.IO.File.Delete(path2 + "\\original files\\" + Path.GetFileName(str1));
            System.IO.File.Move(str1, path2 + "\\original files\\" + Path.GetFileName(str1));
          }
          catch (Exception ex)
          {
            try
            {
              System.IO.File.Move(str1, path2 + "\\rejects\\" + Path.GetFileName(str1));
            }
            catch
            {
            }
          }
        }
      }
      catch (Exception ex)
      {
      }
      try
      {
        foreach (string str1 in new ArrayList((ICollection) Directory.GetFiles(path3)))
        {
          string contents = Regex.Replace(System.IO.File.ReadAllText(str1, Encoding.Default), ",(?=[^\"]*\"(?:[^\"]*\"[^\"]*\")*[^\"]*$)", string.Empty);
          string str2 = Path.GetFileNameWithoutExtension(str1) + "_amended" + Common.timestamp() + ".csv";
          try
          {
            System.IO.File.AppendAllText(path3 + "\\tempfiles\\" + str2, contents, Encoding.Default);
            System.IO.File.Move(path3 + "\\tempfiles\\" + str2, path3 + "\\processed\\" + str2);
            if (System.IO.File.Exists(path3 + "\\original files\\" + Path.GetFileName(str1)))
              System.IO.File.Delete(path3 + "\\original files\\" + Path.GetFileName(str1));
            System.IO.File.Move(str1, path3 + "\\original files\\" + Path.GetFileName(str1));
          }
          catch (Exception ex)
          {
            try
            {
              System.IO.File.Move(str1, path3 + "\\rejects\\" + Path.GetFileName(str1));
            }
            catch
            {
            }
          }
        }
      }
      catch (Exception ex)
      {
      }
      try
      {
        foreach (string str1 in new ArrayList((ICollection) Directory.GetFiles(path4)))
        {
          string contents = Regex.Replace(System.IO.File.ReadAllText(str1, Encoding.Default), ",(?=[^\"]*\"(?:[^\"]*\"[^\"]*\")*[^\"]*$)", string.Empty);
          string str2 = Path.GetFileNameWithoutExtension(str1) + "_amended" + Common.timestamp() + ".csv";
          try
          {
            System.IO.File.AppendAllText(path4 + "\\tempfiles\\" + str2, contents, Encoding.Default);
            System.IO.File.Move(path4 + "\\tempfiles\\" + str2, path4 + "\\processed\\" + str2);
            if (System.IO.File.Exists(path4 + "\\original files\\" + Path.GetFileName(str1)))
              System.IO.File.Delete(path4 + "\\original files\\" + Path.GetFileName(str1));
            System.IO.File.Move(str1, path4 + "\\original files\\" + Path.GetFileName(str1));
          }
          catch (Exception ex)
          {
            try
            {
              System.IO.File.Move(str1, path4 + "\\rejects\\" + Path.GetFileName(str1));
            }
            catch
            {
            }
          }
        }
      }
      catch (Exception ex)
      {
      }
      try
      {
        foreach (string str1 in new ArrayList((ICollection) Directory.GetFiles(path1)))
        {
          string str2 = System.IO.File.ReadAllText(str1, Encoding.Default).Replace("\n", "\r\n");
          int startIndex = str2.IndexOf("Product");
          string contents = str2.Substring(startIndex, str2.Length - startIndex);
          string str3 = Path.GetFileNameWithoutExtension(str1) + "_amended" + Common.timestamp() + ".xls";
          try
          {
            System.IO.File.AppendAllText(path1 + "\\tempfiles\\" + str3, contents, Encoding.Default);
            System.IO.File.Move(path1 + "\\tempfiles\\" + str3, path1 + "\\processed\\" + str3);
            if (System.IO.File.Exists(path1 + "\\original files\\" + Path.GetFileName(str1)))
              System.IO.File.Delete(path1 + "\\original files\\" + Path.GetFileName(str1));
            System.IO.File.Move(str1, path1 + "\\original files\\" + Path.GetFileName(str1));
          }
          catch (Exception ex)
          {
            try
            {
              System.IO.File.Move(str1, path1 + "\\rejects\\" + Path.GetFileName(str1));
            }
            catch
            {
            }
          }
        }
      }
      catch (Exception ex)
      {
      }
    }

    private void oustandingFileCounts()
    {
      string path1 = "\\\\10.16.72.129\\company\\FTP\\root\\MSESRVDOM\\apple\\In\\Oracle Purchase Files\\";
      string path2 = "\\\\10.16.72.129\\company\\FTP\\root\\MSESRVDOM\\apple\\In\\Product Range Files\\";
      string path3 = "\\\\10.16.72.129\\company\\FTP\\root\\MSESRVDOM\\apple\\In\\Oracle Inventory\\";
      string path4 = "\\\\10.16.72.129\\company\\FTP\\root\\MSESRVDOM\\apple\\In\\Oustanding Hub Orders\\";
      string path5 = "\\\\10.16.72.129\\company\\FTP\\root\\MSESRVDOM\\apple\\In\\Apple Inventory\\IT\\";
      string path6 = "\\\\10.16.72.129\\company\\FTP\\root\\MSESRVDOM\\apple\\In\\Apple Inventory\\UK\\";
      string path7 = "\\\\10.16.72.129\\company\\FTP\\root\\MSESRVDOM\\apple\\In\\Apple Inventory\\UAE\\";
      string path8 = "\\\\10.16.72.129\\company\\FTP\\root\\MSESRVDOM\\apple\\In\\Apple Inventory\\NL\\";
      string path9 = "\\\\10.16.72.129\\company\\FTP\\root\\MSESRVDOM\\apple\\In\\Apple Forecast\\";
      string path10 = "\\\\10.16.72.129\\company\\FTP\\root\\MSESRVDOM\\apple\\In\\Apple VMI Report\\";
      ArrayList arrayList1 = new ArrayList((ICollection) Directory.GetFiles(path9));
      ArrayList arrayList2 = new ArrayList((ICollection) Directory.GetFiles(path10));
      ArrayList arrayList3 = new ArrayList((ICollection) Directory.GetFiles(path1));
      ArrayList arrayList4 = new ArrayList((ICollection) Directory.GetFiles(path2));
      ArrayList arrayList5 = new ArrayList((ICollection) Directory.GetFiles(path3));
      ArrayList arrayList6 = new ArrayList((ICollection) Directory.GetFiles(path4));
      ArrayList arrayList7 = new ArrayList((ICollection) Directory.GetFiles(path6));
      ArrayList arrayList8 = new ArrayList((ICollection) Directory.GetFiles(path7));
      ArrayList arrayList9 = new ArrayList((ICollection) Directory.GetFiles(path8));
      ArrayList arrayList10 = new ArrayList((ICollection) Directory.GetFiles(path5));
      int count1 = arrayList7.Count;
      int count2 = arrayList8.Count;
      int count3 = arrayList9.Count;
      this.lblIT.Text = arrayList10.Count.ToString();
      this.lblUAE.Text = count2.ToString();
      this.lblNL.Text = count3.ToString();
      this.lblUK.Text = count1.ToString();
      this.lblPO.Text = arrayList3.Count.ToString();
      this.lblPR.Text = arrayList4.Count.ToString();
      this.lblOS.Text = arrayList5.Count.ToString();
      this.lblBO.Text = arrayList6.Count.ToString();
    }

    private void procesOracleFiles()
    {
      string path1 = "\\\\10.16.72.129\\company\\FTP\\root\\MSESRVDOM\\apple\\In\\Oracle Purchase Files\\";
      string path2 = "\\\\10.16.72.129\\company\\FTP\\root\\MSESRVDOM\\apple\\In\\Product Range Files\\";
      string path3 = "\\\\10.16.72.129\\company\\FTP\\root\\MSESRVDOM\\apple\\In\\Oracle Inventory\\";
      string path4 = "\\\\10.16.72.129\\company\\FTP\\root\\MSESRVDOM\\apple\\In\\Oustanding Hub Orders\\";
      ArrayList arrayList1 = new ArrayList((ICollection) Directory.GetFiles(path1));
      ArrayList arrayList2 = new ArrayList((ICollection) Directory.GetFiles(path2));
      ArrayList arrayList3 = new ArrayList((ICollection) Directory.GetFiles(path3));
      ArrayList arrayList4 = new ArrayList((ICollection) Directory.GetFiles(path4));
      foreach (string str1 in arrayList2)
      {
        string contents = Regex.Replace(System.IO.File.ReadAllText(str1, Encoding.Default), ",(?=[^\"]*\"(?:[^\"]*\"[^\"]*\")*[^\"]*$)", string.Empty);
        string str2 = Path.GetFileNameWithoutExtension(str1) + "_amended" + Common.timestamp() + ".csv";
        try
        {
          System.IO.File.AppendAllText(path2 + "\\tempfiles\\" + str2, contents, Encoding.Default);
          System.IO.File.Move(path2 + "\\tempfiles\\" + str2, path2 + "\\processed\\" + str2);
          System.IO.File.Move(str1, path2 + "\\original files\\" + Path.GetFileName(str1));
        }
        catch (Exception ex)
        {
          try
          {
            System.IO.File.Move(str1, path2 + "\\rejects\\" + Path.GetFileName(str1));
          }
          catch
          {
          }
        }
      }
      foreach (string str1 in arrayList1)
      {
        string contents = Regex.Replace(System.IO.File.ReadAllText(str1, Encoding.Default), ",(?=[^\"]*\"(?:[^\"]*\"[^\"]*\")*[^\"]*$)", string.Empty);
        string str2 = Path.GetFileNameWithoutExtension(str1) + "_amended4" + Common.timestamp() + ".csv";
        try
        {
          System.IO.File.AppendAllText(path1 + "\\tempfiles\\" + str2, contents, Encoding.Default);
          System.IO.File.Move(path1 + "\\tempfiles\\" + str2, path1 + "\\processed\\" + str2);
          System.IO.File.Delete(str1);
        }
        catch (Exception ex)
        {
          try
          {
            System.IO.File.Move(str1, path1 + "\\rejects\\" + Path.GetFileName(str1));
          }
          catch
          {
          }
        }
      }
      foreach (string str1 in arrayList3)
      {
        string contents = Regex.Replace(System.IO.File.ReadAllText(str1, Encoding.Default), ",(?=[^\"]*\"(?:[^\"]*\"[^\"]*\")*[^\"]*$)", string.Empty);
        string str2 = Path.GetFileNameWithoutExtension(str1) + "_amended" + Common.timestamp() + ".csv";
        try
        {
          System.IO.File.AppendAllText(path3 + "\\tempfiles\\" + str2, contents, Encoding.Default);
          System.IO.File.Move(path3 + "\\tempfiles\\" + str2, path3 + "\\processed\\" + str2);
          System.IO.File.Delete(str1);
        }
        catch (Exception ex)
        {
          try
          {
            System.IO.File.Move(str1, path3 + "\\rejects\\" + Path.GetFileName(str1));
          }
          catch
          {
          }
        }
      }
      foreach (string str1 in arrayList4)
      {
        string contents = Regex.Replace(System.IO.File.ReadAllText(str1, Encoding.Default), ",(?=[^\"]*\"(?:[^\"]*\"[^\"]*\")*[^\"]*$)", string.Empty).Replace("\r\n\n", "\r\n");
        string str2 = Path.GetFileNameWithoutExtension(str1) + "_amended" + Common.timestamp() + ".csv";
        try
        {
          System.IO.File.AppendAllText(path4 + "\\tempfiles\\" + str2, contents, Encoding.Default);
          System.IO.File.Move(path4 + "\\tempfiles\\" + str2, path4 + "\\processed\\" + str2);
          System.IO.File.Move(str1, path4 + "\\original files\\" + Path.GetFileName(str1));
        }
        catch (Exception ex)
        {
          try
          {
            System.IO.File.Move(str1, path4 + "\\rejects\\" + Path.GetFileName(str1));
          }
          catch
          {
          }
        }
      }
    }

    

    private void processOtherAppleFiles()
    {
      string path1 = "\\\\10.16.72.129\\company\\FTP\\root\\MSESRVDOM\\apple\\In\\Apple Forecast\\";
      string path2 = "\\\\10.16.72.129\\company\\FTP\\root\\MSESRVDOM\\apple\\In\\Apple VMI Report\\";
      ArrayList arrayList1 = new ArrayList((ICollection) Directory.GetFiles(path1));
      ArrayList arrayList2 = new ArrayList((ICollection) Directory.GetFiles(path2));
      foreach (string str1 in arrayList1)
      {
        string contents = System.IO.File.ReadAllText(str1, Encoding.Default).Replace(",\n", ",\"\"\n");
        string str2 = Path.GetFileNameWithoutExtension(str1) + "_amended" + Common.timestamp() + ".csv";
        try
        {
          System.IO.File.AppendAllText(path1 + "\\tempfiles\\" + str2, contents, Encoding.Default);
          System.IO.File.Move(path1 + "\\tempfiles\\" + str2, path1 + "\\processed\\" + str2);
          System.IO.File.Move(str1, path1 + "\\original files\\" + Path.GetFileName(str1));
        }
        catch (Exception ex)
        {
          try
          {
            System.IO.File.Move(str1, path1 + "\\rejects\\" + Path.GetFileName(str1));
          }
          catch
          {
          }
        }
      }
      foreach (string str1 in arrayList2)
      {
        string contents = Regex.Replace(System.IO.File.ReadAllText(str1, Encoding.Default), ",(?=[^\"]*\"(?:[^\"]*\"[^\"]*\")*[^\"]*$)", string.Empty);
        string str2 = Path.GetFileNameWithoutExtension(str1) + "_amended" + Common.timestamp() + ".csv";
        try
        {
          System.IO.File.AppendAllText(path2 + "\\tempfiles\\" + str2, contents, Encoding.Default);
          System.IO.File.Move(path2 + "\\tempfiles\\" + str2, path2 + "\\processed\\" + str2);
          System.IO.File.Move(str1, path2 + "\\original files\\" + Path.GetFileName(str1));
        }
        catch (Exception ex)
        {
          try
          {
            System.IO.File.Move(str1, path2 + "\\rejects\\" + Path.GetFileName(str1));
          }
          catch
          {
          }
        }
      }
    }

    

   

    private void createForecastCommitResponse(int reportID, bool testing)
    {
      string filename = Common.runSQLScalar(string.Format("select substring(FileName,0,39) from MSE_AppleForecastCommitReports where reportid={0}", (object) reportID)).ToString() + ".csv";
      this.runReport(string.Format("sp_appleforecastcommitresponse {0}\r\n", (object) reportID), filename);
    }

    protected void btnProcessAllFiles_Click(object sender, EventArgs e)
    {
      try
      {
        this.processInventoryFiles();
      }
      catch
      {
      }
      try
      {
        this.processOtherAppleFiles();
      }
      catch
      {
      }
      try
      {
        this.procesOracleFiles();
      }
      catch
      {
      }
    }



    protected void gvLastImportedOracle_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            System.Web.UI.WebControls.Image theImage = (System.Web.UI.WebControls.Image)e.Row.FindControl("imgImportStatus");
            if (int.Parse(DataBinder.Eval(e.Row.DataItem, "dateDiffImport").ToString()) > 24)
            {
                theImage.ImageUrl = "~/images/x.png";
            }
            else
            {
                theImage.ImageUrl = "~/images/tick.png";
            }
        }

    }

    

    protected void gvLatestInventory_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {

            string date = DataBinder.Eval(e.Row.DataItem, "DateCreated").ToString();
            DateTime  dt = Convert.ToDateTime(date);
            TimeSpan ts = (DateTime.Now-dt);


            System.Web.UI.WebControls.Image theImage = (System.Web.UI.WebControls.Image)e.Row.FindControl("imgImportStatus");
            if (ts.TotalHours > 24)
            {
                theImage.ImageUrl = "~/images/x.png";
            }
            else
            {
                theImage.ImageUrl = "~/images/tick.png";
            }
        }
    }

    protected void gvVMI_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {

            string date = DataBinder.Eval(e.Row.DataItem, "DateCreated").ToString();
            DateTime dt = Convert.ToDateTime(date);
            TimeSpan ts = (DateTime.Now - dt);


            System.Web.UI.WebControls.Image theImage = (System.Web.UI.WebControls.Image)e.Row.FindControl("imgImportStatus");
            if (ts.TotalHours > 72)
            {
                theImage.ImageUrl = "~/images/x.png";
            }
            else
            {
                theImage.ImageUrl = "~/images/tick.png";
            }
        }
    }

    protected void gvFC_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {

            string date = DataBinder.Eval(e.Row.DataItem, "ReportDate").ToString();
            DateTime dt = Convert.ToDateTime(date);
            TimeSpan ts = (DateTime.Now - dt);


            System.Web.UI.WebControls.Image theImage = (System.Web.UI.WebControls.Image)e.Row.FindControl("imgImportStatus");
            if (ts.TotalHours > 100)
            {
                theImage.ImageUrl = "~/images/x.png";
            }
            else
            {
                theImage.ImageUrl = "~/images/tick.png";
            }
        }
    }
    protected void btnDownloadInvBalance_Command(object sender, CommandEventArgs e)
    {
        string str1 = e.CommandArgument.ToString();

        this.runReport("select * from vw_appleinventorybalancereport where InventoryReportID=" + str1, "Inventory_Balance_ReportID_" + str1 + "_" + Common.timestamp() + ".csv");

    }
  }
}
