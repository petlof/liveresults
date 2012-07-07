<?php

$lang = "en";
if (isset($_GET['lang']) && file_exists('help_'.$_GET['lang'].".html"))
{
	$lang = $_GET['lang'];
}

include "help_$lang.html";
?>
