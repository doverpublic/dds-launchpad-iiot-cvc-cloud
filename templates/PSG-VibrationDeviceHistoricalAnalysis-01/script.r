source('./r_files/flatten_HTML.r')

############### Library Declarations ###############
libraryRequireInstall("ggplot2");
libraryRequireInstall("plotly")
####################################################

################### Actual code ####################
# Create the data frame
m <- list(
  l = 20,
  r = 20,
  b = 20,
  t = 20,
  pad = 4
)

g <- plot_ly(Values, x = Values$TimestampIndex , y = Values$Frequency , z = Values$Magnitude, type = 'scatter3d', xcalendar = "gregorian", mode = 'markers', marker=list(size=4), surfaceaxis = "0", name="Vibration Analysis - 3D Historical", text="Time Period,Frequency,Magnitude", surfaceaxis = "2", opacity = 1) %>% layout(autosize = F, width = 900, height = 600, margin = m)

####################################################

############# Create and save widget ###############
p = ggplotly(g);
internalSaveWidget(p, 'out.html');
####################################################
