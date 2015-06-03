<?php
// From PHPParser doc:
// "This ensures that there will be no errors when traversing highly nested node trees."
ini_set('xdebug.max_nesting_level', 2000);


require __DIR__ . '/../vendor/autoload.php';

use PHPAnalyzer\Commands\GreetCommand;
use PHPAnalyzer\Commands\ParseCommand;
use PHPAnalyzer\Commands\ScanCommand;
use PHPAnalyzer\Commands\CountCommand;
use Symfony\Component\Console\Application;

$application = new Application();
$commands = array(new GreetCommand(),
                  new CountCommand(),
                  //new ScanCommand(),
                  new ParseCommand());

$application->addCommands($commands);

$application->run();