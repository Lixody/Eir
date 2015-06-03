<?php


namespace PHPAnalyzer\Commands;


use Symfony\Component\Console\Command\Command;
use Symfony\Component\Console\Input\InputArgument;
use Symfony\Component\Console\Input\InputInterface;
use Symfony\Component\Console\Input\InputOption;
use Symfony\Component\Console\Output\OutputInterface;
use PHPAnalyzer\Utils;
use Symfony\Component\Finder\Finder;

class CountCommand extends Command{
    protected function configure() {
        $this->setName('count')
             ->setDescription('Compute code file statistics (Require CLOC)')
             ->addArgument('target', InputArgument::REQUIRED, 'Target directory of scan.')
             ->addOption('ext', 'e', InputArgument::IS_ARRAY | InputArgument::OPTIONAL, 'Count files with specified extensions.', null)
             //->addOption('subdirs', 'sub', InputOption::VALUE_OPTIONAL, "Include subdirectories when counting.", true);
             ;
    }

    protected function execute(InputInterface $input, OutputInterface $output) {
        $target = $input->getArgument('target');

        if ( !is_dir($target) ) {
            $output->writeln("Error: Target does not seem to be a valid directory..");
            return;
        }

        $output->writeln("Starting directory scan of " . $target);
        system("cloc " . $target);

        //$output->writeln(iterator_count($this->GetFiles($target)));
        //$output->writeln("");

        //foreach ($this->GetFiles($target) as $file) {
        //    $output->writeln($file);
        //}
    }

    private function GetFiles($dir) {
        $finder = new Finder();
        return $finder->files()->in($dir);
    }
}