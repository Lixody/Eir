<?php
namespace PHPAnalyzer\Commands;

use PhpParser\Error;
use PhpParser\Lexer;
use PhpParser\Parser;
use PhpParser\Serializer\XML;
use Symfony\Component\Console\Command\Command;
use Symfony\Component\Console\Input\InputArgument;
use Symfony\Component\Console\Input\InputInterface;
use Symfony\Component\Console\Input\InputOption;
use Symfony\Component\Console\Output\OutputInterface;
use Symfony\Component\Finder\Finder;
ini_set('display_errors', 'On');
error_reporting(E_ALL);
class ParseCommand extends Command{
    private $sourceArgName = 'source';
    private $destinationArgName = 'destination';

    protected function configure() {
        $this->setName('parse')
             ->setDescription('Start PHP file parsing.')
             ->addArgument($this->sourceArgName, InputArgument::REQUIRED, 'Target file to scan and parse.')
             ->addArgument($this->destinationArgName, InputArgument::OPTIONAL, 'Destination file. If not set, output will be printed to standard output')
             ;
    }

    protected function execute(InputInterface $input, OutputInterface $output) {
        $source = $input->getArgument($this->sourceArgName);
        $destinationDir = $input->getArgument($this->destinationArgName);

        if (is_dir($source)) {
            $this->parseDirectory($source, $destinationDir);
        } else if (file_exists($source)) {
            $parseResult = $this->parseFile($source);
            $this->outputParseResult($parseResult, $destinationDir);

        } else {
            $output->writeln("ERROR: Target does not seem to be a valid file.. (Target was: '" . $source . "')");
        }
    }

    private function parseFile($file) {
        $code = file_get_contents($file);

        $parser = new Parser(new Lexer());
        $serializer = new XML();

        try {
            $stmts = $parser->parse($code);
            $serialized = $serializer->serialize($stmts);
            return $serialized;
        } catch (Error $e) {
            echo "Parser error: " . $e->getMessage();
            //$output->writeln('Parser error: ' . $e->getMessage());
        }
    }

    private function outputParseResult($output, $destination) {
        if (isset($destination)) {
            file_put_contents($destination, $output);
            return;
        }
        echo $output;
    }





    private function parseDirectory($source, $destination) {
        $finder = new Finder();
        $finder->files()->in($source);

        //$output->writeln('Found ' . $finder->name('.php')->count() . ' PHP files out of ' . $finder->count() . ' total.');

        //foreach ($finder as $file) {
        //    $output->writeln($file);
        //}

        $parser = new Parser(new Lexer());

        try {
            $stmts = $finder = new Finder();
            $finder->files()->in($source);

            //$output->writeln('Found ' . $finder->name('.php')->count() . ' PHP files out of ' . $finder->count() . ' total.');

            //foreach ($finder as $file) {
            //    $output->writeln($file);
            //}

        } catch (Error $e) {
            //$output->writeln('Parse error: ', $e->getMessage());
            return;
        }
    }
}